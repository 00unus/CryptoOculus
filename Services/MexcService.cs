using CryptoOculus.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class MexcService(IHttpClientFactory httpClientFactory, ILogger<MexcService> logger, ApiKeysService apiKeys, IWebHostEnvironment env) : IExchange, IDnsUpdate
    {
        public int ExchangeId { get; } = 1;
        public string ExchangeName { get; } = "Mexc";
        public string[] Hosts { get; } = ["api.mexc.com"];
        public string[] Ips { get; set; } = [];

        private void ValidateMexcExchangeInfo(HttpRequestMessage request)
        {
            _ = ClientService.Deserialize<MexcExchangeInfo>(request);
        }
        private void ValidateMexcContractAddress(HttpRequestMessage request)
        {
            _ = ClientService.Deserialize<MexcContractAddress[]>(request);
        }
        private void ValidateMexcPrice(HttpRequestMessage request)
        {
            _ = ClientService.Deserialize<MexcPrice[]>(request);
        }
        private void ValidateMexcOrderBook(HttpRequestMessage request)
        {
            _ = ClientService.Deserialize<MexcOrderBook>(request);
        }

        public async Task<Pair[]> GetPairs()
        {
            //Getting Current exchange trading rules and symbol information (Exchange info)
            async Task<MexcExchangeInfo> ExInfo()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/v3/exchangeInfo").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateMexcExchangeInfo);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<MexcExchangeInfo>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currency details and the smart contract address
            async Task<MexcContractAddress[]> Contract()
            {
                using HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(apiKeys.GetSingle("MexcSecretKey")));
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"recvWindow=20000&timestamp={now}"));

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/v3/capital/config/getall?recvWindow=20000&timestamp={now}&signature={Convert.ToHexStringLower(hash)}").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Headers.Add("X-MEXC-APIKEY", apiKeys.GetSingle("MexcApiKey"));
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateMexcContractAddress);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<MexcContractAddress[]>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currect prices of pairs
            async Task<MexcPrice[]> Prices()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/v3/ticker/bookTicker").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateMexcPrice);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<MexcPrice[]?>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            try
            {
                Task<MexcExchangeInfo> exInfoTask = ExInfo();
                Task<MexcContractAddress[]> contractTask = Contract();
                Task<MexcPrice[]> pricesTask = Prices();

                await Task.WhenAll([exInfoTask, contractTask, pricesTask]);

                MexcExchangeInfo exchangeInfo = await exInfoTask;
                MexcContractAddress[] contractAddresses = await contractTask;
                MexcPrice[] prices = await pricesTask;

                List<Pair> pairs = [];

                for (int i = 0; i < exchangeInfo.Symbols.Length; i++)
                {
                    if (exchangeInfo.Symbols[i].Status == "1" &&
                       (exchangeInfo.Symbols[i].QuoteAsset == "USDT" || exchangeInfo.Symbols[i].QuoteAsset == "USDC" || exchangeInfo.Symbols[i].QuoteAsset == "TUSD"))
                    {
                        //Adding basic info of pair
                        Pair pair = new()
                        {
                            ExchangeId = ExchangeId,
                            ExchangeName = ExchangeName,
                            BaseAsset = exchangeInfo.Symbols[i].BaseAsset.ToUpper(),
                            QuoteAsset = exchangeInfo.Symbols[i].QuoteAsset.ToUpper()
                        };

                        //adding price of pair
                        for (int a = 0; a < prices.Length; a++)
                        {
                            if (prices[a].Symbol == exchangeInfo.Symbols[i].Symbol)
                            {
                                if (double.TryParse(prices[a].AskPrice, out double askPrice))
                                {
                                    pair.AskPrice = askPrice;
                                }

                                if (double.TryParse(prices[a].BidPrice, out double bidPrice))
                                {
                                    pair.BidPrice = bidPrice;
                                }

                                break;
                            }
                        }

                        //adding supported networks of base asset
                        for (int b = 0; b < contractAddresses.Length; b++)
                        {
                            if (contractAddresses[b].Coin == exchangeInfo.Symbols[i].BaseAsset)
                            {
                                List<AssetNetwork> baseAssetNetworks = [];
                                for (int c = 0; c < contractAddresses[b].NetworkList.Length; c++)
                                {
                                    if (contractAddresses[b].NetworkList[c].DepositEnable || contractAddresses[b].NetworkList[c].WithdrawEnable)
                                    {
                                        AssetNetwork assetNetwork = new()
                                        {
                                            NetworkName = contractAddresses[b].NetworkList[c].Network
                                        };

                                        if (!String.IsNullOrWhiteSpace(contractAddresses[b].NetworkList[c].Contract))
                                        {
                                            assetNetwork.Address = contractAddresses[b].NetworkList[c].Contract;
                                        }

                                        if (double.TryParse(contractAddresses[b].NetworkList[c].WithdrawFee, out double withdrawFee))
                                        {
                                            if (pair.AskPrice != 0)
                                            {
                                                assetNetwork.WithdrawFee = pair.AskPrice * withdrawFee;
                                            }

                                            else if (pair.BidPrice != 0)
                                            {
                                                assetNetwork.WithdrawFee = pair.BidPrice * withdrawFee;
                                            }
                                        }

                                        assetNetwork.DepositEnable = contractAddresses[b].NetworkList[c].DepositEnable;
                                        assetNetwork.WithdrawEnable = contractAddresses[b].NetworkList[c].WithdrawEnable;
                                        baseAssetNetworks.Add(assetNetwork);
                                    }
                                }

                                if (baseAssetNetworks.Count > 0 && (pair.AskPrice != 0 && pair.BidPrice != 0))
                                {
                                    pair.BaseAssetNetworks = [.. baseAssetNetworks];
                                }

                                break;
                            }
                        }

                        if ((pair.BidPrice != 0 || pair.AskPrice != 0) && pair.BaseAssetNetworks.Length > 0)
                        {
                            pairs.Add(pair);
                        }
                    }
                }

                using StreamWriter sw = new(Path.Combine(env.ContentRootPath, "Cache/Mexc/firstStepPairs.json"));
                sw.Write(JsonSerializer.Serialize<List<Pair>>(pairs, Helper.serializeOptions));

                return [.. pairs];
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Service disabled!");
                return [];
            }
        }

        public async Task<double[][]?> OrderBook(string baseAsset, string quoteAsset, bool askOrBid)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/v3/depth?symbol={baseAsset.ToUpper()}{quoteAsset.ToUpper()}&limit=100").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateMexcOrderBook);
                string clientName = ClientService.RotatingProxyClient("mexc");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                MexcOrderBook orderBook = JsonSerializer.Deserialize<MexcOrderBook>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                if (askOrBid)
                {
                    return Array.ConvertAll(orderBook.Asks, innerArray => Array.ConvertAll(innerArray, double.Parse));
                }

                else if (!askOrBid)
                {
                    return Array.ConvertAll(orderBook.Bids, innerArray => Array.ConvertAll(innerArray, double.Parse));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "OrderBook disabled!");
            }

            return null;
        }
    }
}
