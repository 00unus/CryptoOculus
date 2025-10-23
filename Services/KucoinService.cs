using CryptoOculus.Models;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class KucoinService(IHttpClientFactory httpClientFactory, ILogger<KucoinService> logger, IWebHostEnvironment env) : IExchange, IDnsUpdate
    {
        public int ExchangeId { get; } = 7;
        public string ExchangeName { get; } = "KuCoin";
        public string[] Hosts { get; } = ["api.kucoin.com", "www.kucoin.com"];
        public string[] Ips { get; set; } = [];

        private void ValidateKucoinExchangeInfo(HttpRequestMessage request)
        {
            KucoinExchangeInfo model = ClientService.Deserialize<KucoinExchangeInfo>(request);

            if (model.Data is null || model.Code != "200000")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateKucoinContractAddresses(HttpRequestMessage request)
        {
            KucoinContractAddresses model = ClientService.Deserialize<KucoinContractAddresses>(request);

            if (model.Data is null || model.Code != "200000")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateKucoinPrices(HttpRequestMessage request)
        {
            KucoinPrices model = ClientService.Deserialize<KucoinPrices>(request);

            if (model.Data is null || model.Code != "200000")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateKucoinFeeConfig(HttpRequestMessage request)
        {
            KucoinFeeConfig model = ClientService.Deserialize<KucoinFeeConfig>(request);

            if (model.Data is null || model.Code != "200" || model.Msg != "success" || !model.Success)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateKucoinOrderBook(HttpRequestMessage request)
        {
            KucoinOrderBook model = ClientService.Deserialize<KucoinOrderBook>(request);

            if (model.Data is null || model.Code != "200000")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }

        public async Task<Pair[]> GetPairs()
        {
            //Getting Current exchange trading rules and symbol information (Exchange info)
            async Task<KucoinExchangeInfo> ExInfo()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/v2/symbols").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateKucoinExchangeInfo);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<KucoinExchangeInfo>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currency details and the smart contract address
            async Task<KucoinContractAddresses> Contract()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/v3/currencies").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateKucoinContractAddresses);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<KucoinContractAddresses>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currect prices of pairs
            async Task<KucoinPrices> Prices()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/v1/market/allTickers").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateKucoinPrices);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<KucoinPrices>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query comission
            async Task<KucoinFeeConfig> FeeConfig()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[1]}/_api/trade-marketing/feeConfig/groupedConfig").WithVersion();
                request.Headers.Host = Hosts[1];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateKucoinFeeConfig);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<KucoinFeeConfig>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            try
            {
                Task<KucoinExchangeInfo> exInfoTask = ExInfo();
                Task<KucoinContractAddresses> contractTask = Contract();
                Task<KucoinPrices> pricesTask = Prices();
                Task<KucoinFeeConfig> feeConfigTask = FeeConfig();

                await Task.WhenAll([exInfoTask, contractTask, pricesTask, feeConfigTask]);

                KucoinExchangeInfo exchangeInfo = await exInfoTask;
                KucoinContractAddresses contractAddresses = await contractTask;
                KucoinPrices prices = await pricesTask;
                KucoinFeeConfig feeConfig = await feeConfigTask;

                List<Pair> pairs = [];

                if (exchangeInfo.Data is not null)
                {
                    for (int i = 0; i < exchangeInfo.Data.Length; i++)
                    {
                        if (exchangeInfo.Data[i].EnableTrading == true &&
                           (exchangeInfo.Data[i].QuoteCurrency == "USDT" || exchangeInfo.Data[i].QuoteCurrency == "USDC" || exchangeInfo.Data[i].QuoteCurrency == "TUSD"))
                        {
                            //Adding basic info of pair
                            Pair pair = new()
                            {
                                ExchangeId = ExchangeId,
                                ExchangeName = ExchangeName,
                                BaseAsset = exchangeInfo.Data[i].BaseCurrency.ToUpper(),
                                QuoteAsset = exchangeInfo.Data[i].QuoteCurrency.ToUpper(),
                                Url = $"https://www.kucoin.com/trade/{exchangeInfo.Data[i].BaseCurrency.ToUpper()}-{exchangeInfo.Data[i].QuoteCurrency.ToUpper()}"
                            };

                            //Adding commision
                            if (feeConfig.Data is not null)
                            {
                                for (int a = 0; a < feeConfig.Data.Length; a++)
                                {
                                    if (feeConfig.Data[a].GroupLevel == exchangeInfo.Data[i].FeeCategory)
                                    {
                                        pair.SpotTakerComission = double.Parse(feeConfig.Data[a].TradeFeeConfigs[0].SpotTakerFee);
                                        break;
                                    }
                                }
                            }

                            //adding price of pair
                            if (prices.Data is not null)
                            {
                                for (int a = 0; a < prices.Data.Ticker.Length; a++)
                                {
                                    if (prices.Data.Ticker[a].Symbol.Equals(exchangeInfo.Data[i].Symbol, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        if (double.TryParse(prices.Data.Ticker[a].Sell, out double askPrice))
                                        {
                                            pair.AskPrice = askPrice;
                                        }

                                        if (double.TryParse(prices.Data.Ticker[a].Buy, out double bidPrice))
                                        {
                                            pair.BidPrice = bidPrice;
                                        }

                                        break;
                                    }
                                }
                            }

                            //adding supported networks of base asset
                            if (contractAddresses is not null && contractAddresses.Data is not null)
                            {
                                for (int b = 0; b < contractAddresses.Data.Length; b++)
                                {
                                    if (contractAddresses.Data[b].Currency.Equals(exchangeInfo.Data[i].BaseCurrency, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        List<AssetNetwork> baseAssetNetworks = [];
                                        for (int c = 0; c < contractAddresses.Data[b].Chains.Length; c++)
                                        {
                                            if (contractAddresses.Data[b].Chains[c].IsDepositEnabled || contractAddresses.Data[b].Chains[c].IsWithdrawEnabled)
                                            {
                                                AssetNetwork assetNetwork = new()
                                                {
                                                    NetworkName = contractAddresses.Data[b].Chains[c].ChainName,
                                                    DepositEnable = contractAddresses.Data[b].Chains[c].IsDepositEnabled,
                                                    WithdrawEnable = contractAddresses.Data[b].Chains[c].IsWithdrawEnabled,
                                                    DepositUrl = $"https://www.kucoin.com/assets/coin/{contractAddresses.Data[b].Currency.ToUpper()}",
                                                    WithdrawUrl = $"https://www.kucoin.com/assets/withdraw/{contractAddresses.Data[b].Currency.ToUpper()}"
                                                };

                                                if (!String.IsNullOrWhiteSpace(contractAddresses.Data[b].Chains[c].ContractAddress))
                                                {
                                                    assetNetwork.Address = contractAddresses.Data[b].Chains[c].ContractAddress;
                                                }

                                                //Withraw fee
                                                if (double.TryParse(contractAddresses.Data[b].Chains[c].WithdrawalMinFee, out double withdrawFee))
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
                                                if (double.TryParse(contractAddresses.Data[b].Chains[c].WithdrawFeeRate, out double WithdrawFeeRate))
                                                {
                                                    assetNetwork.TransferTax = WithdrawFeeRate;
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

                await File.WriteAllTextAsync(Path.Combine(env.ContentRootPath, "Cache/kucoin.json"), JsonSerializer.Serialize(pairs, Helper.serializeOptions));

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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/v1/market/orderbook/level2_100?symbol={baseAsset.ToUpper()}-{quoteAsset.ToUpper()}").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateKucoinOrderBook);
                string clientName = ClientService.RotatingProxyClient("kucoin");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                KucoinOrderBook orderBook = JsonSerializer.Deserialize<KucoinOrderBook>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

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
