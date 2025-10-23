using CryptoOculus.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class BingxService(IHttpClientFactory httpClientFactory, ILogger<BingxService> logger, ApiKeysService apiKeys, IWebHostEnvironment env) : IExchange, IDnsUpdate
    {
        public int ExchangeId { get; } = 6;
        public string ExchangeName { get; } = "BingX";
        public string[] Hosts { get; } = ["open-api.bingx.com"];
        public string[] Ips { get; set; } = [];

        private void ValidateBingxExchangeInfo(HttpRequestMessage request)
        {
            BingxExchangeInfo model = ClientService.Deserialize<BingxExchangeInfo>(request);

            if (model.Data is null || model.Code != 0)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateBingxContractAddresses(HttpRequestMessage request)
        {
            BingxContractAddresses model = ClientService.Deserialize<BingxContractAddresses>(request);

            if (model.Data is null || model.Code != 0)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateBingxPrices(HttpRequestMessage request)
        {
            BingxPrices model = ClientService.Deserialize<BingxPrices>(request);

            if (model.Data is null || model.Code != 0)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateBingxCommissions(HttpRequestMessage request)
        {
            BingxCommission model = ClientService.Deserialize<BingxCommission>(request);

            if (model.Data is null || model.Code != 0)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateBingxOrderBook(HttpRequestMessage request)
        {
            BingxOrderBook model = ClientService.Deserialize<BingxOrderBook>(request);

            if (model.Data is null || model.Code != 0)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }

        public async Task<Pair[]> GetPairs()
        {
            //Getting Current exchange trading rules and symbol information (Exchange info)
            async Task<BingxExchangeInfo> ExInfo()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/openApi/spot/v1/common/symbols").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.LimitRuleName, "BingxIpGroup1");
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateBingxExchangeInfo);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<BingxExchangeInfo>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currency details and the smart contract address
            async Task<BingxContractAddresses> Contract()
            {
                using HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(apiKeys.GetSingle("BingxSecretKey")));
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"timestamp={now}"));

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/openApi/wallets/v1/capital/config/getall?timestamp={now}&signature={Convert.ToHexStringLower(computedHash)}").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Headers.Add("X-BX-APIKEY", apiKeys.GetSingle("BingxApiKey"));
                request.Options.Set(HttpOptionKeys.LimitRuleName, "BingxIpGroup2");
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateBingxContractAddresses);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<BingxContractAddresses>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currect prices of pairs
            async Task<BingxPrices> Prices()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/openApi/spot/v1/ticker/bookTicker").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.LimitRuleName, "BingxIpGroup1");
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateBingxPrices);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<BingxPrices>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query comission
            /*async Task<BingxCommission> Commissions()
            {
                using HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(apiKeys.GetSingle("BingxSecretKey")));
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"timestamp={now}&symbol=BTC-USDT"));

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/openApi/spot/v1/user/commissionRate?timestamp={now}&symbol=BTC-USDT&signature={Convert.ToHexStringLower(computedHash)}").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Headers.Add("X-BX-APIKEY", apiKeys.GetSingle("BingxApiKey"));
                request.Options.Set(HttpOptionKeys.LimitRuleName, "BingxIpGroup2");
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateBingxCommissions);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);
                Console.WriteLine(await response.Content.ReadAsStringAsync());
                return JsonSerializer.Deserialize<BingxCommission>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }*/

            try
            {
                Task<BingxExchangeInfo> exInfoTask = ExInfo();
                Task<BingxContractAddresses> contractTask = Contract();
                Task<BingxPrices> pricesTask = Prices();
                //Task<BingxCommission> commissionsTask = Commissions();

                await Task.WhenAll([exInfoTask, contractTask, pricesTask]);

                BingxExchangeInfo exchangeInfo = await exInfoTask;
                BingxContractAddresses contractAddresses = await contractTask;
                BingxPrices prices = await pricesTask;
                //BingxCommission commissions = await commissionsTask;

                List<Pair> pairs = [];

                if (exchangeInfo.Data is not null)
                {
                    for (int i = 0; i < exchangeInfo.Data.Symbols.Length; i++)
                    {
                        //Splitting symbol to base asset and quote asset
                        string[] symbol = exchangeInfo.Data.Symbols[i].Symbol.Split('-');
                        string baseAsset = symbol[0];
                        string quoteAsset = symbol[1];

                        if (exchangeInfo.Data.Symbols[i].Status == 1 &&
                           (quoteAsset == "USDT" || quoteAsset == "USDC" || quoteAsset == "TUSD"))
                        {
                            //Adding basic info of pair
                            Pair pair = new()
                            {
                                ExchangeId = ExchangeId,
                                ExchangeName = ExchangeName,
                                BaseAsset = baseAsset.ToUpper(),
                                QuoteAsset = quoteAsset.ToUpper(),
                                Url = $"https://bingx.com/spot/{baseAsset.ToUpper()}{quoteAsset.ToUpper()}",
                                SpotTakerComission = 0.001
                            };

                            //adding price of pair
                            if (prices.Data is not null)
                            {
                                for (int a = 0; a < prices.Data.Length; a++)
                                {
                                    if (prices.Data[a].Symbol.Equals(exchangeInfo.Data.Symbols[i].Symbol, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        if (double.TryParse(prices.Data[a].AskPrice, out double askPrice))
                                        {
                                            pair.AskPrice = askPrice;
                                        }

                                        if (double.TryParse(prices.Data[a].BidPrice, out double bidPrice))
                                        {
                                            pair.BidPrice = bidPrice;
                                        }

                                        break;
                                    }
                                }
                            }

                            //adding supported networks of base asset
                            if (contractAddresses.Data is not null)
                            {
                                for (int b = 0; b < contractAddresses.Data.Length; b++)
                                {
                                    if (contractAddresses.Data[b].Coin.Equals(baseAsset, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        List<AssetNetwork> baseAssetNetworks = [];
                                        for (int c = 0; c < contractAddresses.Data[b].NetworkList.Length; c++)
                                        {
                                            if (contractAddresses.Data[b].NetworkList[c].DepositEnable || contractAddresses.Data[b].NetworkList[c].WithdrawEnable)
                                            {
                                                AssetNetwork assetNetwork = new()
                                                {
                                                    NetworkName = contractAddresses.Data[b].NetworkList[c].Network,
                                                    DepositEnable = contractAddresses.Data[b].NetworkList[c].DepositEnable,
                                                    WithdrawEnable = contractAddresses.Data[b].NetworkList[c].WithdrawEnable,
                                                    DepositUrl = "https://bingx.com/assets/recharge",
                                                    WithdrawUrl = "https://bingx.com/assets/withdraw"
                                                };

                                                //Withraw fee
                                                if (double.TryParse(contractAddresses.Data[b].NetworkList[c].WithdrawFee, out double withdrawFee))
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

                                                if (!String.IsNullOrWhiteSpace(contractAddresses.Data[b].NetworkList[c].ContractAddress))
                                                {
                                                    assetNetwork.Address = contractAddresses.Data[b].NetworkList[c].ContractAddress;
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
                            }

                            if ((pair.BidPrice != 0 || pair.AskPrice != 0) && pair.BaseAssetNetworks.Length > 0)
                            {
                                pairs.Add(pair);
                            }
                        }
                    }
                }

                await File.WriteAllTextAsync(Path.Combine(env.ContentRootPath, "Cache/bingx.json"), JsonSerializer.Serialize(pairs, Helper.serializeOptions));

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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/openApi/spot/v1/market/depth?symbol={baseAsset.ToUpper()}-{quoteAsset.ToUpper()}&limit=100").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateBingxOrderBook);
                string clientName = ClientService.RotatingProxyClient("bingx");
                request.Options.Set(HttpOptionKeys.LimitRuleName, "BingxIpGroup1");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                BingxOrderBook orderBook = JsonSerializer.Deserialize<BingxOrderBook>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                if (askOrBid && orderBook.Data is not null)
                {
                    return Array.ConvertAll(orderBook.Data.Asks, innerArray => Array.ConvertAll(innerArray, double.Parse));
                }

                else if (!askOrBid && orderBook.Data is not null)
                {
                    return Array.ConvertAll(orderBook.Data.Bids, innerArray => Array.ConvertAll(innerArray, double.Parse));
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
