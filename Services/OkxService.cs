using CryptoOculus.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class OkxService(IHttpClientFactory httpClientFactory, ILogger<OkxService> logger, ApiKeysService apiKeys, IWebHostEnvironment env) : IExchange, IDnsUpdate
    {
        public int ExchangeId { get; } = 9;
        public string ExchangeName { get; } = "OKX";
        public string[] Hosts { get; } = ["www.okx.com"];
        public string[] Ips { get; set; } = [];

        private void ValidateOkxExchangeInfo(HttpRequestMessage request)
        {
            OkxExchangeInfo model = ClientService.Deserialize<OkxExchangeInfo>(request);

            if (model.Data is null || model.Code != "0")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateOkxContractAddresses(HttpRequestMessage request)
        {
            OkxContractAddresses model = ClientService.Deserialize<OkxContractAddresses>(request);

            if (model.Data is null || model.Code != "0")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateOkxPrices(HttpRequestMessage request)
        {
            OkxPrices model = ClientService.Deserialize<OkxPrices>(request);

            if (model.Data is null || model.Code != "0")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateOkxOrderBook(HttpRequestMessage request)
        {
            OkxOrderBook model = ClientService.Deserialize<OkxOrderBook>(request);

            if (model.Data is null || model.Code != "0")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }

        public async Task<Pair[]> GetPairs()
        {
            //Getting Current exchange trading rules and symbol information (Exchange info)
            async Task<OkxExchangeInfo> ExInfo()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/v5/public/instruments?instType=SPOT").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateOkxExchangeInfo);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<OkxExchangeInfo>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currency details and the smart contract address
            async Task<OkxContractAddresses> Contract()
            {
                string now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.sssZ");
                byte[] computedHash = new HMACSHA256(Encoding.UTF8.GetBytes(apiKeys.GetSingle("OkxSecretKey"))).ComputeHash(Encoding.UTF8.GetBytes($"{now}GET/api/v5/asset/currencies"));

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/v5/asset/currencies").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Headers.Add("OK-ACCESS-KEY", apiKeys.GetSingle("OkxApiKey"));
                request.Headers.Add("OK-ACCESS-SIGN", Convert.ToBase64String(computedHash));
                request.Headers.Add("OK-ACCESS-PASSPHRASE", apiKeys.GetSingle("OkxPassKey"));
                request.Headers.Add("OK-ACCESS-TIMESTAMP", now);
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateOkxContractAddresses);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<OkxContractAddresses>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currect prices of pairs
            async Task<OkxPrices> Prices()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/v5/market/tickers?instType=SPOT").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateOkxPrices);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<OkxPrices>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            try
            {
                Task<OkxExchangeInfo> exInfoTask = ExInfo();
                Task<OkxContractAddresses> contractTask = Contract();
                Task<OkxPrices> pricesTask = Prices();

                await Task.WhenAll([exInfoTask, contractTask, pricesTask]);

                OkxExchangeInfo exchangeInfo = await exInfoTask;
                OkxContractAddresses contractAddresses = await contractTask;
                OkxPrices prices = await pricesTask;

                List<Pair> pairs = [];

                if (exchangeInfo.Data is not null)
                {
                    for (int i = 0; i < exchangeInfo.Data.Length; i++)
                    {
                        if (exchangeInfo.Data[i].State == "live" &&
                           (exchangeInfo.Data[i].QuoteCcy == "USDT" || exchangeInfo.Data[i].QuoteCcy == "USDC" || exchangeInfo.Data[i].QuoteCcy == "TUSD"))
                        {
                            //Adding basic info of pair
                            Pair pair = new()
                            {
                                ExchangeId = ExchangeId,
                                ExchangeName = ExchangeName,
                                BaseAsset = exchangeInfo.Data[i].BaseCcy.ToUpper(),
                                QuoteAsset = exchangeInfo.Data[i].QuoteCcy.ToUpper()
                            };

                            //adding price of pair
                            if (prices.Data is not null)
                            {
                                for (int a = 0; a < prices.Data.Length; a++)
                                {
                                    if (prices.Data[a].InstId == exchangeInfo.Data[i].InstId)
                                    {
                                        if (double.TryParse(prices.Data[a].AskPx, out double askPrice))
                                        {
                                            pair.AskPrice = askPrice;
                                        }

                                        if (double.TryParse(prices.Data[a].BidPx, out double bidPrice))
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
                                List<AssetNetwork> baseAssetNetworks = [];

                                for (int b = 0; b < contractAddresses.Data.Length; b++)
                                {
                                    if (contractAddresses.Data[b].Ccy == exchangeInfo.Data[i].BaseCcy)
                                    {
                                        if (contractAddresses.Data[b].CanDep || contractAddresses.Data[b].CanWd)
                                        {
                                            string[] splitChainName = contractAddresses.Data[b].Chain.Split(['-'], 2);
                                            AssetNetwork assetNetwork = new()
                                            {
                                                NetworkName = splitChainName[1],
                                                DepositEnable = contractAddresses.Data[b].CanDep,
                                                WithdrawEnable = contractAddresses.Data[b].CanWd,
                                                TransferTax = 0
                                            };

                                            if (!String.IsNullOrWhiteSpace(contractAddresses.Data[b].CtAddr))
                                            {
                                                assetNetwork.Address = contractAddresses.Data[b].CtAddr;
                                            }

                                            //Withraw fee
                                            if (double.TryParse(contractAddresses.Data[b].Fee, out double withdrawFee))
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

                                            //Blockchain transfer tax
                                            if (double.TryParse(contractAddresses.Data[b].BurningFeeRate, out double burningFeeRate))
                                            {
                                                assetNetwork.TransferTax = burningFeeRate;
                                            }

                                            baseAssetNetworks.Add(assetNetwork);
                                        }
                                    }
                                }

                                if (baseAssetNetworks.Count > 0)
                                {
                                    pair.BaseAssetNetworks = [.. baseAssetNetworks];
                                }
                            }

                            if ((pair.BidPrice != 0 || pair.AskPrice != 0) && pair.BaseAssetNetworks.Length > 0)
                            {
                                pairs.Add(pair);
                            }
                        }
                    }
                }

                using StreamWriter sw = new(Path.Combine(env.ContentRootPath, "Cache/Okx/firstStepPairs.json"));
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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/v5/market/books?instId={baseAsset}-{quoteAsset}&sz=100").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateOkxOrderBook);
                string clientName = ClientService.RotatingProxyClient("coinw");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);


                OkxOrderBook orderBook = JsonSerializer.Deserialize<OkxOrderBook>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                if (askOrBid && orderBook.Data is not null)
                {
                    return Array.ConvertAll(orderBook.Data[0].Asks, innerArray => Array.ConvertAll(innerArray, double.Parse));
                }

                else if (!askOrBid && orderBook.Data is not null)
                {
                    return Array.ConvertAll(orderBook.Data[0].Bids, innerArray => Array.ConvertAll(innerArray, double.Parse));
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
