using CryptoOculus.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class BinanceService(IHttpClientFactory httpClientFactory, ILogger<BinanceService> logger, ApiKeysService apiKeys, IWebHostEnvironment env) : IExchange, IDnsUpdate
    {
        public int ExchangeId { get; } = 0;
        public string ExchangeName { get; } = "Binance";
        public string[] Hosts { get; } = ["api.binance.com"];
        public string[] Ips { get; set; } = [];

        private void ValidateBinanceExchangeInfo(HttpRequestMessage request)
        {
            _ = ClientService.Deserialize<BinanceExchangeInfo>(request);
        }
        private void ValidateBinanceContractAddresses(HttpRequestMessage request)
        {
            _ = ClientService.Deserialize<BinanceContractAddresses[]>(request);
        }
        private void ValidateBinancePrice(HttpRequestMessage request)
        {
            _ = ClientService.Deserialize<BinancePrice[]>(request);
        }
        private void ValidateBinanceOrderBook(HttpRequestMessage request)
        {
            _ = ClientService.Deserialize<BinanceOrderBook>(request);
        }

        public async Task<Pair[]> GetPairs()
        {
            //Getting Current exchange trading rules and symbol information (Exchange info)
            async Task<BinanceExchangeInfo> ExInfo()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/v3/exchangeInfo").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateBinanceExchangeInfo);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<BinanceExchangeInfo>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currency details and the smart contract address
            async Task<BinanceContractAddresses[]> Contract()
            {
                using HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(apiKeys.GetSingle("BinanceSecretKey")!));
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"recvWindow=20000&timestamp={now}"));

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/sapi/v1/capital/config/getall?recvWindow=20000&timestamp={now}&signature={Convert.ToHexStringLower(computedHash)}").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Headers.Add("X-MBX-APIKEY", apiKeys.GetSingle("BinanceApiKey"));
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateBinanceContractAddresses);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<BinanceContractAddresses[]>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currect prices of pairs
            async Task<BinancePrice[]> Prices()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/v3/ticker/24hr").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateBinancePrice);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<BinancePrice[]>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            try
            {
                Task<BinanceExchangeInfo> exInfoTask = ExInfo();
                Task<BinanceContractAddresses[]> contractTask = Contract();
                Task<BinancePrice[]> pricesTask = Prices();

                await Task.WhenAll([exInfoTask, contractTask, pricesTask]);

                BinanceExchangeInfo exchangeInfo = await exInfoTask;
                BinanceContractAddresses[] contractAddresses = await contractTask;
                BinancePrice[] prices = await pricesTask;

                List<Pair> pairs = [];

                for (int i = 0; i < exchangeInfo.Symbols.Length; i++)
                {
                    if (exchangeInfo.Symbols[i].Status == "TRADING" &&
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
                                            NetworkName = contractAddresses[b].NetworkList[c].Network,
                                            DepositEnable = contractAddresses[b].NetworkList[c].DepositEnable,
                                            WithdrawEnable = contractAddresses[b].NetworkList[c].WithdrawEnable
                                        };

                                        //Withraw fee
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

                                        if (!String.IsNullOrWhiteSpace(contractAddresses[b].NetworkList[c].ContractAddress))
                                        {
                                            assetNetwork.Address = contractAddresses[b].NetworkList[c].ContractAddress;
                                        }

                                        baseAssetNetworks.Add(assetNetwork);
                                    }
                                }

                                if (baseAssetNetworks.Count > 0)
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

                using StreamWriter sw = new(Path.Combine(env.ContentRootPath, "Cache/Binance/firstStepPairs.json"));
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
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateBinanceOrderBook);
                string clientName = ClientService.RotatingProxyClient("binance");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                BinanceOrderBook orderBook = JsonSerializer.Deserialize<BinanceOrderBook>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

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
                logger.LogWarning(ex, "OrderBook disabled!");
            }

            return null;
        }
    }
}
