using CryptoOculus.Models;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class XtcomService(IHttpClientFactory httpClientFactory, ILogger<XtcomService> logger, IWebHostEnvironment env) : IExchange, IDnsUpdate
    {
        public int ExchangeId { get; } = 14;
        public string ExchangeName { get; } = "XT․com";
        public string[] Hosts { get; } = ["sapi.xt.com"];
        public string[] Ips { get; set; } = [];

        private void ValidateXtcomExchangeInfo(HttpRequestMessage request)
        {
            XtcomExchangeInfo model = ClientService.Deserialize<XtcomExchangeInfo>(request);

            if (model.Result is null || model.Rc != 0 || model.Mc != "SUCCESS")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateXtcomContractAddresses(HttpRequestMessage request)
        {
            XtcomContractAddresses model = ClientService.Deserialize<XtcomContractAddresses>(request);

            if (model.Result is null || model.Rc != 0 || model.Mc != "SUCCESS")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateXtcomPrices(HttpRequestMessage request)
        {
            XtcomPrices model = ClientService.Deserialize<XtcomPrices>(request);

            if (model.Result is null || model.Rc != 0 || model.Mc != "SUCCESS")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateXtcomOrderBook(HttpRequestMessage request)
        {
            XtcomOrderBook model = ClientService.Deserialize<XtcomOrderBook>(request);

            if (model.Result is null || model.Rc != 0 || model.Mc != "SUCCESS")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }

        public async Task<Pair[]> GetPairs()
        {
            //Getting Current exchange trading rules and symbol information (Exchange info)
            async Task<XtcomExchangeInfo> ExInfo()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v4/public/symbol").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateXtcomExchangeInfo);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<XtcomExchangeInfo>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currency details and the smart contract address
            async Task<XtcomContractAddresses> Contract()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v4/public/wallet/support/currency").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateXtcomContractAddresses);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<XtcomContractAddresses>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currect prices of pairs
            async Task<XtcomPrices> Prices()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v4/public/ticker").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateXtcomPrices);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<XtcomPrices>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            try
            {
                Task<XtcomExchangeInfo> exInfoTask = ExInfo();
                Task<XtcomContractAddresses> contractTask = Contract();
                Task<XtcomPrices> pricesTask = Prices();

                await Task.WhenAll([exInfoTask, contractTask, pricesTask]);

                XtcomExchangeInfo exchangeInfo = await exInfoTask;
                XtcomContractAddresses contractAddresses = await contractTask;
                XtcomPrices prices = await pricesTask;

                List<Pair> pairs = [];

                if (exchangeInfo.Result is not null)
                {
                    for (int i = 0; i < exchangeInfo.Result.Symbols.Length; i++)
                    {
                        if (exchangeInfo.Result.Symbols[i].State == "ONLINE" &&
                           (exchangeInfo.Result.Symbols[i].QuoteCurrency == "usdt" || exchangeInfo.Result.Symbols[i].QuoteCurrency == "usdc" || exchangeInfo.Result.Symbols[i].QuoteCurrency == "tusd"))
                        {
                            //Adding basic info of pair
                            Pair pair = new()
                            {
                                ExchangeId = ExchangeId,
                                ExchangeName = ExchangeName,
                                BaseAsset = exchangeInfo.Result.Symbols[i].BaseCurrency.ToUpper(),
                                QuoteAsset = exchangeInfo.Result.Symbols[i].QuoteCurrency.ToUpper()
                            };

                            //adding price of pair
                            if (prices.Result is not null)
                            {
                                for (int a = 0; a < prices.Result.Length; a++)
                                {
                                    if (prices.Result[a].S.Equals(exchangeInfo.Result.Symbols[i].Symbol, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        if (double.TryParse(prices.Result[a].Ap, out double askPrice))
                                        {
                                            pair.AskPrice = askPrice;
                                        }

                                        if (double.TryParse(prices.Result[a].Bp, out double bidPrice))
                                        {
                                            pair.BidPrice = bidPrice;
                                        }

                                        break;
                                    }
                                }
                            }

                            //adding supported networks of base asset
                            if (contractAddresses.Result is not null)
                            {
                                for (int b = 0; b < contractAddresses.Result.Length; b++)
                                {
                                    if (contractAddresses.Result[b].Currency.Equals(exchangeInfo.Result.Symbols[i].BaseCurrency, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        List<AssetNetwork> baseAssetNetworks = [];
                                        for (int c = 0; c < contractAddresses.Result[b].SupportChains.Length; c++)
                                        {
                                            if (contractAddresses.Result[b].SupportChains[c].DepositEnabled || contractAddresses.Result[b].SupportChains[c].WithdrawEnabled)
                                            {
                                                AssetNetwork assetNetwork = new()
                                                {
                                                    NetworkName = contractAddresses.Result[b].SupportChains[c].Chain,
                                                    DepositEnable = contractAddresses.Result[b].SupportChains[c].DepositEnabled,
                                                    WithdrawEnable = contractAddresses.Result[b].SupportChains[c].WithdrawEnabled,
                                                    DepositTax = contractAddresses.Result[b].SupportChains[c].DepositFeeRate
                                                };

                                                if (!String.IsNullOrWhiteSpace(contractAddresses.Result[b].SupportChains[c].Contract))
                                                {
                                                    assetNetwork.Address = contractAddresses.Result[b].SupportChains[c].Contract.Replace(" ", "");
                                                }

                                                //Withraw fee
                                                if (pair.AskPrice != 0)
                                                {
                                                    assetNetwork.WithdrawFee = pair.AskPrice * contractAddresses.Result[b].SupportChains[c].WithdrawFeeAmount;
                                                }

                                                else if (pair.BidPrice != 0)
                                                {
                                                    assetNetwork.WithdrawFee = pair.BidPrice * contractAddresses.Result[b].SupportChains[c].WithdrawFeeAmount;
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

                using StreamWriter sw = new(Path.Combine(env.ContentRootPath, "Cache/Xtcom/firstStepPairs.json"));
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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v4/public/depth?symbol={baseAsset.ToLower()}_{quoteAsset.ToLower()}&limit=100").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateXtcomOrderBook);
                string clientName = ClientService.RotatingProxyClient("xtcom");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                XtcomOrderBook orderBook = JsonSerializer.Deserialize<XtcomOrderBook>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                if (askOrBid && orderBook.Result is not null && orderBook.Result.Asks is not null)
                {
                    return Array.ConvertAll(orderBook.Result.Asks, innerArray => Array.ConvertAll(innerArray, double.Parse));
                }

                else if (!askOrBid && orderBook.Result is not null && orderBook.Result.Bids is not null)
                {
                    return Array.ConvertAll(orderBook.Result.Bids, innerArray => Array.ConvertAll(innerArray, double.Parse));
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
