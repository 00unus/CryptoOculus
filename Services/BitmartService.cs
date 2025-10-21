using CryptoOculus.Models;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class BitmartService(IHttpClientFactory httpClientFactory, ILogger<BitmartService> logger, IWebHostEnvironment env) : IExchange, IDnsUpdate
    {
        public int ExchangeId { get; } = 5;
        public string ExchangeName { get; } = "BitMart";
        public string[] Hosts { get; } = ["api-cloud.bitmart.com"];
        public string[] Ips { get; set; } = [];

        private void ValidateBitmartExchangeInfo(HttpRequestMessage request)
        {
            BitmartExchangeInfo model = ClientService.Deserialize<BitmartExchangeInfo>(request);

            if (model.Data is null || model.Code != 1000 || model.Message != "OK")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateBitmartContractAddress(HttpRequestMessage request)
        {
            BitmartContractAddress model = ClientService.Deserialize<BitmartContractAddress>(request);

            if (model.Data is null || model.Code != 1000 || model.Message != "OK")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateBitmartPrices(HttpRequestMessage request)
        {
            BitmartPrices model = ClientService.Deserialize<BitmartPrices>(request);

            if (model.Data is null || model.Code != 1000 || model.Message != "success")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateBitmartOrderBook(HttpRequestMessage request)
        {
            BitmartOrderBook model = ClientService.Deserialize<BitmartOrderBook>(request);

            if (model.Data is null || model.Code != 1000 || model.Message != "success")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }

        public async Task<Pair[]> GetPairs()
        {
            //Getting Current exchange trading rules and symbol information (Exchange info)
            async Task<BitmartExchangeInfo> ExInfo()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/spot/v1/symbols/details").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateBitmartExchangeInfo);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<BitmartExchangeInfo>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currency details and the smart contract address
            async Task<BitmartContractAddress> Contract()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/account/v1/currencies").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateBitmartContractAddress);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<BitmartContractAddress>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currect prices of pairs
            async Task<BitmartPrices> Prices()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/spot/quotation/v3/tickers").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateBitmartPrices);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<BitmartPrices>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            try
            {
                Task<BitmartExchangeInfo> exInfoTask = ExInfo();
                Task<BitmartContractAddress> contractTask = Contract();
                Task<BitmartPrices> pricesTask = Prices();

                await Task.WhenAll([exInfoTask, contractTask, pricesTask]);

                BitmartExchangeInfo exchangeInfo = await exInfoTask;
                BitmartContractAddress contractAddresses = await contractTask;
                BitmartPrices prices = await pricesTask;

                List<Pair> pairs = [];

                if (exchangeInfo.Data is not null)
                {
                    for (int i = 0; i < exchangeInfo.Data.Symbols.Length; i++)
                    {
                        if (exchangeInfo.Data.Symbols[i].Trade_status == "trading" &&
                           (exchangeInfo.Data.Symbols[i].Quote_currency == "USDT" || exchangeInfo.Data.Symbols[i].Quote_currency == "USDC" || exchangeInfo.Data.Symbols[i].Quote_currency == "TUSD"))
                        {
                            //Adding basic info of pair
                            Pair pair = new()
                            {
                                ExchangeId = ExchangeId,
                                ExchangeName = ExchangeName,
                                BaseAsset = exchangeInfo.Data.Symbols[i].Base_currency,
                                QuoteAsset = exchangeInfo.Data.Symbols[i].Quote_currency
                            };

                            //adding price of pair
                            if (prices.Data is not null)
                            {
                                for (int a = 0; a < prices.Data.Length; a++)
                                {
                                    if (prices.Data[a][0] == exchangeInfo.Data.Symbols[i].Symbol)
                                    {
                                        if (double.TryParse(prices.Data[a][10], out double askPrice))
                                        {
                                            pair.AskPrice = askPrice;
                                        }

                                        if (double.TryParse(prices.Data[a][8], out double bidPrice))
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

                                for (int b = 0; b < contractAddresses.Data.Currencies.Length; b++)
                                {
                                    string[] split = contractAddresses.Data.Currencies[b].Currency.Split("-");
                                    if (split[0] == exchangeInfo.Data.Symbols[i].Base_currency)
                                    {
                                        if (contractAddresses.Data.Currencies[b].Deposit_enabled || contractAddresses.Data.Currencies[b].Withdraw_enabled)
                                        {
                                            AssetNetwork assetNetwork = new()
                                            {
                                                NetworkName = contractAddresses.Data.Currencies[b].Network,
                                                DepositEnable = contractAddresses.Data.Currencies[b].Deposit_enabled,
                                                WithdrawEnable = contractAddresses.Data.Currencies[b].Withdraw_enabled
                                            };

                                            //Withraw fee
                                            if (double.TryParse(contractAddresses.Data.Currencies[b].Withdraw_minfee, out double withdrawFee))
                                            {
                                                if (pair.AskPrice != 0)
                                                {
                                                    assetNetwork.WithdrawFee = withdrawFee;
                                                }

                                                else if (pair.BidPrice != 0)
                                                {
                                                    assetNetwork.WithdrawFee = withdrawFee;
                                                }
                                            }

                                            if (!String.IsNullOrWhiteSpace(contractAddresses.Data.Currencies[b].Contract_address))
                                            {
                                                assetNetwork.Address = contractAddresses.Data.Currencies[b].Contract_address;
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

                //Remove pairs with empty prices
                for (int i = 0; i < pairs.Count; i++)
                {
                    if (pairs[i].BidPrice == 0 || pairs[i].AskPrice == 0)
                    {
                        pairs.RemoveAt(i);
                        i--;
                    }
                }

                using StreamWriter sw = new(Path.Combine(env.ContentRootPath, "Cache/Bitmart/firstStepPairs.json"));
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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/spot/quotation/v3/books?symbol={baseAsset.ToUpper()}_{quoteAsset.ToUpper()}&limit=50").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateBitmartOrderBook);
                string clientName = ClientService.RotatingProxyClient("bitmart");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                BitmartOrderBook orderBook = JsonSerializer.Deserialize<BitmartOrderBook>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

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
