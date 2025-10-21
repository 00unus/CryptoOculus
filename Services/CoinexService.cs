using CryptoOculus.Models;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class CoinexService(IHttpClientFactory httpClientFactory, ILogger<CoinexService> logger, IWebHostEnvironment env) : IExchange, IDnsUpdate
    {
        public int ExchangeId { get; } = 12;
        public string ExchangeName { get; } = "CoinEx";
        public string[] Hosts { get; } = ["api.coinex.com"];
        public string[] Ips { get; set; } = [];

        private void ValidateCoinexExchangeInfo(HttpRequestMessage request)
        {
            CoinexExchangeInfo model = ClientService.Deserialize<CoinexExchangeInfo>(request);

            if (model.Data is null || model.Code != 0 || model.Message != "OK")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateCoinexContractAddresses(HttpRequestMessage request)
        {
            CoinexContractAddresses model = ClientService.Deserialize<CoinexContractAddresses>(request);

            if (model.Data is null || model.Code != 0 || model.Message != "OK")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateCoinexCurrencyDetails(HttpRequestMessage request)
        {
            CoinexCurrencyDetails model = ClientService.Deserialize<CoinexCurrencyDetails>(request);

            if (model.Data is null || model.Code != 0 || model.Message != "OK")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateCoinexPrices(HttpRequestMessage request)
        {
            CoinexPrices model = ClientService.Deserialize<CoinexPrices>(request);

            if (model.Data is null || model.Code != 0 || model.Message != "OK")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateCoinexOrderBook(HttpRequestMessage request)
        {
            CoinexOrderBook model = ClientService.Deserialize<CoinexOrderBook>(request);

            if (model.Data is null || model.Code != 0 || model.Message != "OK")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }

        public async Task<Pair[]> GetPairs()
        {
            //Getting Current exchange trading rules and symbol information (Exchange info)
            async Task<CoinexExchangeInfo> ExInfo()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v2/spot/market").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateCoinexExchangeInfo);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<CoinexExchangeInfo>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query the smart contract address
            async Task<CoinexContractAddresses> Contract()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v2/assets/info").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateCoinexContractAddresses);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<CoinexContractAddresses>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currency details
            async Task<CoinexCurrencyDetails> Details()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v2/assets/all-deposit-withdraw-config").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateCoinexCurrencyDetails);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<CoinexCurrencyDetails>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currect prices of pairs
            async Task<CoinexPrices> Prices()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v2/spot/ticker").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateCoinexPrices);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<CoinexPrices>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            try
            {
                Task<CoinexExchangeInfo> exInfoTask = ExInfo();
                Task<CoinexContractAddresses> contractTask = Contract();
                Task<CoinexCurrencyDetails> detailsTask = Details();
                Task<CoinexPrices> pricesTask = Prices();

                await Task.WhenAll([exInfoTask, contractTask, pricesTask]);

                CoinexExchangeInfo exchangeInfo = await exInfoTask;
                CoinexContractAddresses contractAddresses = await contractTask;
                CoinexCurrencyDetails currencyDetails = await detailsTask;
                CoinexPrices prices = await pricesTask;

                List<Pair> pairs = [];

                if (exchangeInfo.Data is not null)
                {
                    for (int i = 0; i < exchangeInfo.Data.Length; i++)
                    {
                        if (exchangeInfo.Data[i].QuoteCcy == "USDT" || exchangeInfo.Data[i].QuoteCcy == "USDC" || exchangeInfo.Data[i].QuoteCcy == "TUSD")
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
                                    if (prices.Data[a].Market.Equals(exchangeInfo.Data[i].Market, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        if (double.TryParse(prices.Data[a].Last, out double lastPrice))
                                        {
                                            pair.AskPrice = lastPrice;
                                            pair.BidPrice = lastPrice;
                                        }

                                        break;
                                    }
                                }
                            }

                            //adding supported networks of base asset
                            if (currencyDetails.Data is not null && contractAddresses.Data is not null)
                            {
                                for (int b = 0; b < currencyDetails.Data.Length; b++)
                                {
                                    if (currencyDetails.Data[b].Asset.Ccy.Equals(exchangeInfo.Data[i].BaseCcy, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        List<AssetNetwork> baseAssetNetworks = [];
                                        for (int c = 0; c < currencyDetails.Data[b].Chains.Length; c++)
                                        {
                                            if (currencyDetails.Data[b].Chains[c].DepositEnabled || currencyDetails.Data[b].Chains[c].WithdrawEnabled)
                                            {
                                                AssetNetwork assetNetwork = new()
                                                {
                                                    NetworkName = currencyDetails.Data[b].Chains[c].Chain,
                                                    DepositEnable = currencyDetails.Data[b].Chains[c].DepositEnabled,
                                                    WithdrawEnable = currencyDetails.Data[b].Chains[c].WithdrawEnabled
                                                };

                                                //Find contract address
                                                for (int d = 0; d < contractAddresses.Data.Length; d++)
                                                {
                                                    if (contractAddresses.Data[d].ShortName.Equals(exchangeInfo.Data[i].BaseCcy, StringComparison.CurrentCultureIgnoreCase))
                                                    {
                                                        for (int e = 0; e < contractAddresses.Data[d].ChainInfo.Length; e++)
                                                        {
                                                            if (contractAddresses.Data[d].ChainInfo[e].ChainName.Equals(currencyDetails.Data[b].Chains[c].Chain, StringComparison.CurrentCultureIgnoreCase) &&
                                                                !string.IsNullOrWhiteSpace(contractAddresses.Data[d].ChainInfo[e].Identity))
                                                            {
                                                                assetNetwork.Address = contractAddresses.Data[d].ChainInfo[e].Identity;
                                                                break;
                                                            }
                                                        }
                                                        break;
                                                    }
                                                }

                                                //Withraw fee
                                                if (double.TryParse(currencyDetails.Data[b].Chains[c].WithdrawalFee, out double withdrawFee))
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
                                                if (double.TryParse(currencyDetails.Data[b].Chains[c].DeflationRate, out double deflationRate))
                                                {
                                                    assetNetwork.TransferTax = deflationRate;
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

                using StreamWriter sw = new(Path.Combine(env.ContentRootPath, "Cache/Coinex/firstStepPairs.json"));
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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v2/spot/depth?market={baseAsset.ToUpper()}{quoteAsset.ToUpper()}&limit=50&interval=0.000000000001").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateCoinexOrderBook);
                string clientName = ClientService.RotatingProxyClient("coinex");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                CoinexOrderBook orderBook = JsonSerializer.Deserialize<CoinexOrderBook>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                if (askOrBid && orderBook.Data is not null && orderBook.Data.Depth.Asks is not null)
                {
                    return Array.ConvertAll(orderBook.Data.Depth.Asks, innerArray => Array.ConvertAll(innerArray, double.Parse));
                }

                else if (!askOrBid && orderBook.Data is not null && orderBook.Data.Depth.Bids is not null)
                {
                    return Array.ConvertAll(orderBook.Data.Depth.Bids, innerArray => Array.ConvertAll(innerArray, double.Parse));
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