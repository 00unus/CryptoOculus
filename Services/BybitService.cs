using CryptoOculus.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class BybitService(IHttpClientFactory httpClientFactory, ILogger<BybitService> logger, ApiKeysService apiKeys, IWebHostEnvironment env) : IExchange, IDnsUpdate
    {
        public int ExchangeId { get; } = 2;
        public string ExchangeName { get; } = "Bybit";
        public string[] Hosts { get; } = ["api.bybit.com"];
        public string[] Ips { get; set; } = [];

        private void ValidateBybitExchangeInfo(HttpRequestMessage request)
        {
            BybitExchangeInfo model = ClientService.Deserialize<BybitExchangeInfo>(request);

            if (model.Result is null || model.RetCode != 0 || model.RetMsg != "OK")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateBybitContractAddress(HttpRequestMessage request)
        {
            BybitContractAddress model = ClientService.Deserialize<BybitContractAddress>(request);

            if (model.Result is null || model.RetCode != 0 || model.RetMsg != "success")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateBybitPrice(HttpRequestMessage request)
        {
            BybitPrice model = ClientService.Deserialize<BybitPrice>(request);

            if (model.Result is null || model.RetCode != 0 || model.RetMsg != "OK")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateBybitFeeRate(HttpRequestMessage request)
        {
            BybitFeeRate model = ClientService.Deserialize<BybitFeeRate>(request);

            if (model.Result is null || model.RetCode != 0 || model.RetMsg != "OK")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateBybitOrderBook(HttpRequestMessage request)
        {
            BybitOrderBook model = ClientService.Deserialize<BybitOrderBook>(request);

            if (model.Result is null || model.RetCode != 0 || model.RetMsg != "OK")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }

        public async Task<Pair[]> GetPairs()
        {
            //Getting Current exchange trading rules and symbol information (Exchange info)
            async Task<BybitExchangeInfo> ExInfo()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v5/market/instruments-info?category=spot").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateBybitExchangeInfo);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<BybitExchangeInfo>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currency details and the smart contract address
            async Task<BybitContractAddress> Contract()
            {
                HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(apiKeys.GetSingle("BybitSecretKey")));
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                byte[] signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(now + apiKeys.GetSingle("BybitApiKey") + 20000));

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v5/asset/coin/query-info").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Headers.Add("X-BAPI-API-KEY", apiKeys.GetSingle("BybitApiKey"));
                request.Headers.Add("X-BAPI-RECV-WINDOW", "20000");
                request.Headers.Add("X-BAPI-SIGN", Convert.ToHexStringLower(signature));
                request.Headers.Add("X-BAPI-TIMESTAMP", now.ToString());
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateBybitContractAddress);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<BybitContractAddress>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currect prices of pairs
            async Task<BybitPrice> Prices()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v5/market/tickers?category=spot").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateBybitPrice);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<BybitPrice>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query spot commissions
            async Task<BybitFeeRate> FeeRate()
            {
                using HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(apiKeys.GetSingle("BybitSecretKey")));
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                byte[] signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(now + apiKeys.GetSingle("BybitApiKey") + 20000 + "category=spot"));

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v5/account/fee-rate?category=spot").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Headers.Add("X-BAPI-API-KEY", apiKeys.GetSingle("BybitApiKey"));
                request.Headers.Add("X-BAPI-RECV-WINDOW", "20000");
                request.Headers.Add("X-BAPI-SIGN", Convert.ToHexStringLower(signature));
                request.Headers.Add("X-BAPI-TIMESTAMP", now.ToString());
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateBybitFeeRate);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<BybitFeeRate>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            try
            {
                Task<BybitExchangeInfo> exInfoTask = ExInfo();
                Task<BybitContractAddress> contractTask = Contract();
                Task<BybitPrice> pricesTask = Prices();
                Task<BybitFeeRate> feeRateTask = FeeRate();

                await Task.WhenAll([exInfoTask, contractTask, pricesTask, feeRateTask]);

                BybitExchangeInfo exchangeInfo = await exInfoTask;
                BybitContractAddress contractAddresses = await contractTask;
                BybitPrice prices = await pricesTask;
                BybitFeeRate feeRate = await feeRateTask;

                List<Pair> pairs = [];

                if (exchangeInfo.Result is not null)
                {
                    for (int i = 0; i < exchangeInfo.Result.List.Length; i++)
                    {
                        if (exchangeInfo.Result.List[i].Status == "Trading" &&
                           (exchangeInfo.Result.List[i].QuoteCoin == "USDT" || exchangeInfo.Result.List[i].QuoteCoin == "USDC" || exchangeInfo.Result.List[i].QuoteCoin == "TUSD"))
                        {
                            //Adding basic info of pair
                            Pair pair = new()
                            {
                                ExchangeId = ExchangeId,
                                ExchangeName = ExchangeName,
                                BaseAsset = exchangeInfo.Result.List[i].BaseCoin.ToUpper(),
                                QuoteAsset = exchangeInfo.Result.List[i].QuoteCoin.ToUpper(),
                                Url = $"https://www.bybit.com/trade/spot/{exchangeInfo.Result.List[i].BaseCoin.ToUpper()}/{exchangeInfo.Result.List[i].QuoteCoin.ToUpper()}"
                            };

                            //adding spot taker commision
                            if (feeRate.Result is not null)
                            {
                                for (int a = 0; a < feeRate.Result.List.Length; a++)
                                {
                                    if (feeRate.Result.List[a].Symbol.Equals(exchangeInfo.Result.List[i].Symbol))
                                    {
                                        pair.SpotTakerComission = double.Parse(feeRate.Result.List[a].TakerFeeRate);
                                        break;
                                    }
                                }
                            }

                            //adding price of pair
                            if (prices.Result is not null)
                            {
                                for (int a = 0; a < prices.Result.List.Length; a++)
                                {
                                    if (prices.Result.List[a].Symbol.Equals(exchangeInfo.Result.List[i].Symbol, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        if (double.TryParse(prices.Result.List[a].Ask1Price, out double askPrice))
                                        {
                                            pair.AskPrice = askPrice;
                                        }

                                        if (double.TryParse(prices.Result.List[a].Bid1Price, out double bidPrice))
                                        {
                                            pair.BidPrice = bidPrice;
                                        }

                                        break;
                                    }
                                }
                            }

                            //adding supported networks of base asset
                            if (contractAddresses.Result is not null && contractAddresses.Result.Rows is not null)
                            {
                                for (int b = 0; b < contractAddresses.Result.Rows.Length; b++)
                                {
                                    if (contractAddresses.Result.Rows[b].Coin.Equals(exchangeInfo.Result.List[i].BaseCoin, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        List<AssetNetwork> baseAssetNetworks = [];

                                        for (int c = 0; c < contractAddresses.Result.Rows[b].Chains.Length; c++)
                                        {
                                            if (contractAddresses.Result.Rows[b].Chains[c].ChainDeposit == "1" || contractAddresses.Result.Rows[b].Chains[c].ChainWithdraw == "1")
                                            {
                                                AssetNetwork assetNetwork = new()
                                                {
                                                    NetworkName = contractAddresses.Result.Rows[b].Chains[c].Chain,
                                                    DepositEnable = contractAddresses.Result.Rows[b].Chains[c].ChainDeposit == "1",
                                                    WithdrawEnable = contractAddresses.Result.Rows[b].Chains[c].ChainWithdraw == "1",
                                                    DepositUrl = "https://www.bybit.com/user/assets/deposit",
                                                    WithdrawUrl = "https://www.bybit.com/user/assets/withdraw"
                                                };

                                                if (!String.IsNullOrWhiteSpace(contractAddresses.Result.Rows[b].Chains[c].ContractAddress))
                                                {
                                                    assetNetwork.Address = contractAddresses.Result.Rows[b].Chains[c].ContractAddress;
                                                }

                                                //Withraw fee
                                                if (double.TryParse(contractAddresses.Result.Rows[b].Chains[c].WithdrawFee, out double withdrawFee))
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
                                                if (double.TryParse(contractAddresses.Result.Rows[b].Chains[c].WithdrawPercentageFee, out double withdrawPercentageFee))
                                                {
                                                    assetNetwork.TransferTax = withdrawPercentageFee;
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

                await File.WriteAllTextAsync(Path.Combine(env.ContentRootPath, "Cache/bybit.json"), JsonSerializer.Serialize(pairs, Helper.serializeOptions));

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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v5/market/orderbook?category=spot&symbol={baseAsset.ToUpper()}{quoteAsset.ToUpper()}&limit=100").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateBybitOrderBook);
                string clientName = ClientService.RotatingProxyClient("bybit");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                BybitOrderBook orderBook = JsonSerializer.Deserialize<BybitOrderBook>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                if (askOrBid && orderBook.Result is not null)
                {
                    return Array.ConvertAll(orderBook.Result.A, innerArray => Array.ConvertAll(innerArray, double.Parse));
                }

                else if (!askOrBid && orderBook.Result is not null)
                {
                    return Array.ConvertAll(orderBook.Result.B, innerArray => Array.ConvertAll(innerArray, double.Parse));
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