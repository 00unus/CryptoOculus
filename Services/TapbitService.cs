using CryptoOculus.Models;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class TapbitService(IHttpClientFactory httpClientFactory, ILogger<TapbitService> logger, IWebHostEnvironment env) : IExchange, IDnsUpdate
    {
        public int ExchangeId { get; } = 18;
        public string ExchangeName { get; } = "Tapbit";
        public string[] Hosts { get; } = ["openapi.tapbit.com"];
        public string[] Ips { get; set; } = [];

        private void ValidateTapbitExchangeInfo(HttpRequestMessage request)
        {
            TapbitExchangeInfo model = ClientService.Deserialize<TapbitExchangeInfo>(request);

            if (model.Data is null || model.Code != 200)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateTapbitContractAddresses(HttpRequestMessage request)
        {
            TapbitContractAddresses model = ClientService.Deserialize<TapbitContractAddresses>(request);

            if (model.Data is null || model.Code != 200)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateTapbitPrices(HttpRequestMessage request)
        {
            TapbitPrices model = ClientService.Deserialize<TapbitPrices>(request);

            if (model.Data is null || model.Code != 200)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateTapbitOrderBook(HttpRequestMessage request)
        {
            TapbitOrderBook model = ClientService.Deserialize<TapbitOrderBook>(request);

            if (model.Data is null || model.Code != 200)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }

        public async Task<Pair[]> GetPairs()
        {
            //Getting Current exchange trading rules and symbol information (Exchange info)
            async Task<TapbitExchangeInfo> ExInfo()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/spot/api/spot/instruments/trade_pair_list").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateTapbitExchangeInfo);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<TapbitExchangeInfo>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currency details and the smart contract address
            async Task<TapbitContractAddresses> Contract()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/spot/api/spot/instruments/asset/list").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateTapbitContractAddresses);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<TapbitContractAddresses>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currect prices of pairs
            async Task<TapbitPrices> Prices()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/spot/api/spot/instruments/ticker_list").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateTapbitPrices);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<TapbitPrices>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            try
            {
                Task<TapbitExchangeInfo> exInfoTask = ExInfo();
                Task<TapbitContractAddresses> contractTask = Contract();
                Task<TapbitPrices> pricesTask = Prices();

                await Task.WhenAll([exInfoTask, contractTask, pricesTask]);

                TapbitExchangeInfo exchangeInfo = await exInfoTask;
                TapbitContractAddresses contractAddresses = await contractTask;
                TapbitPrices prices = await pricesTask;

                List<Pair> pairs = [];

                if (exchangeInfo.Data is not null)
                {
                    for (int i = 0; i < exchangeInfo.Data.Length; i++)
                    {
                        if (exchangeInfo.Data[i].QuoteAsset.Equals("USDT", StringComparison.CurrentCultureIgnoreCase) || exchangeInfo.Data[i].QuoteAsset.Equals("USDC", StringComparison.CurrentCultureIgnoreCase) || exchangeInfo.Data[i].QuoteAsset.Equals("TUSD", StringComparison.CurrentCultureIgnoreCase))
                        {
                            //Adding basic info of pair
                            Pair pair = new()
                            {
                                ExchangeId = ExchangeId,
                                ExchangeName = ExchangeName,
                                BaseAsset = exchangeInfo.Data[i].BaseAsset.ToUpper(),
                                QuoteAsset = exchangeInfo.Data[i].QuoteAsset.ToUpper()
                            };

                            //adding price of pair
                            if (prices.Data is not null)
                            {
                                for (int a = 0; a < prices.Data.Length; a++)
                                {
                                    if (prices.Data[a].TradePairName.Equals(exchangeInfo.Data[i].TradePairName, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        if (double.TryParse(prices.Data[a].LowestAsk, out double askPrice))
                                        {
                                            pair.AskPrice = askPrice;
                                        }

                                        if (double.TryParse(prices.Data[a].HighestBid, out double bidPrice))
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
                                    if (contractAddresses.Data[b].Currency.Equals(exchangeInfo.Data[i].BaseAsset, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        List<AssetNetwork> baseAssetNetworks = [];
                                        for (int c = 0; c < contractAddresses.Data[b].Chains.Length; c++)
                                        {
                                            if (contractAddresses.Data[b].Chains[c].IsDepositEnabled || contractAddresses.Data[b].Chains[c].IsWithdrawEnabled)
                                            {
                                                AssetNetwork assetNetwork = new()
                                                {
                                                    NetworkName = contractAddresses.Data[b].Chains[c].Chain,
                                                    DepositEnable = contractAddresses.Data[b].Chains[c].IsDepositEnabled,
                                                    WithdrawEnable = contractAddresses.Data[b].Chains[c].IsWithdrawEnabled
                                                };

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

                await File.WriteAllTextAsync(Path.Combine(env.ContentRootPath, "Cache/Tapbit/firstStepPairs.json"), JsonSerializer.Serialize(pairs, Helper.serializeOptions));

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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/spot/api/spot/instruments/depth?instrument_id={baseAsset.ToUpper()}/{quoteAsset.ToUpper()}&depth=100").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateTapbitOrderBook);
                string clientName = ClientService.RotatingProxyClient("tapbit");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                TapbitOrderBook orderBook = JsonSerializer.Deserialize<TapbitOrderBook>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                if (askOrBid && orderBook.Data is not null && orderBook.Data.Asks is not null)
                {
                    return Array.ConvertAll(orderBook.Data.Asks, innerArray => Array.ConvertAll(innerArray, double.Parse));
                }

                else if (!askOrBid && orderBook.Data is not null && orderBook.Data.Bids is not null)
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