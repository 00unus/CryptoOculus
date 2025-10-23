using CryptoOculus.Models;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class BitgetService(IHttpClientFactory httpClientFactory, ILogger<BitgetService> logger, IWebHostEnvironment env) : IExchange, IDnsUpdate
    {
        public int ExchangeId { get; } = 4;
        public string ExchangeName { get; } = "Bitget";
        public string[] Hosts { get; } = ["api.bitget.com"];
        public string[] Ips { get; set; } = [];

        private void ValidateBitgetExchangeInfo(HttpRequestMessage request)
        {
            BitgetExchangeInfo model = ClientService.Deserialize<BitgetExchangeInfo>(request);

            if (model.Data is null || model.Code != "00000" || model.Msg != "success")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateBitgetContractAddresses(HttpRequestMessage request)
        {
            BitgetContractAddresses model = ClientService.Deserialize<BitgetContractAddresses>(request);

            if (model.Data is null || model.Code != "00000" || model.Msg != "success")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateBitgetPrice(HttpRequestMessage request)
        {
            BitgetPrice model = ClientService.Deserialize<BitgetPrice>(request);

            if (model.Data is null || model.Code != "00000" || model.Msg != "success")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateBitgetOrderBook(HttpRequestMessage request)
        {
            BitgetOrderBook model = ClientService.Deserialize<BitgetOrderBook>(request);

            if (model.Data is null || model.Code != "00000" || model.Msg != "success")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }

        public async Task<Pair[]> GetPairs()
        {
            //Getting Current exchange trading rules and symbol information (Exchange info)
            async Task<BitgetExchangeInfo> ExInfo()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/v2/spot/public/symbols").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateBitgetExchangeInfo);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<BitgetExchangeInfo>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currency details and the smart contract address
            async Task<BitgetContractAddresses> Contract()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/v2/spot/public/coins").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateBitgetContractAddresses);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<BitgetContractAddresses>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currect prices of pairs
            async Task<BitgetPrice> Prices()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/v2/spot/market/tickers").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateBitgetPrice);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<BitgetPrice>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            try
            {
                Task<BitgetExchangeInfo> exInfoTask = ExInfo();
                Task<BitgetContractAddresses> contractTask = Contract();
                Task<BitgetPrice> pricesTask = Prices();

                await Task.WhenAll([exInfoTask, contractTask, pricesTask]);

                BitgetExchangeInfo exchangeInfo = await exInfoTask;
                BitgetContractAddresses contractAddresses = await contractTask;
                BitgetPrice prices = await pricesTask;

                List<Pair> pairs = [];

                if (exchangeInfo.Data is not null)
                {
                    for (int i = 0; i < exchangeInfo.Data.Length; i++)
                    {
                        if (exchangeInfo.Data[i].Status == "online" &&
                            (exchangeInfo.Data[i].QuoteCoin == "USDT" || exchangeInfo.Data[i].QuoteCoin == "USDC" || exchangeInfo.Data[i].QuoteCoin == "TUSD"))
                        {
                            //Adding basic info of pair
                            Pair pair = new()
                            {
                                ExchangeId = ExchangeId,
                                ExchangeName = ExchangeName,
                                BaseAsset = exchangeInfo.Data[i].BaseCoin.ToUpper(),
                                QuoteAsset = exchangeInfo.Data[i].QuoteCoin.ToUpper(),
                                Url = $"https://www.bitget.com/spot/{exchangeInfo.Data[i].BaseCoin.ToUpper()}{exchangeInfo.Data[i].QuoteCoin.ToUpper()}",
                                SpotTakerComission = double.Parse(exchangeInfo.Data[i].TakerFeeRate)
                            };

                            //adding price of pair
                            if (prices.Data is not null)
                            {
                                for (int a = 0; a < prices.Data.Length; a++)
                                {
                                    if (prices.Data[a].Symbol.Equals(exchangeInfo.Data[i].Symbol, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        if (double.TryParse(prices.Data[a].AskPr, out double askPrice))
                                        {
                                            pair.AskPrice = askPrice;
                                        }

                                        if (double.TryParse(prices.Data[a].BidPr, out double bidPrice))
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
                                    if (contractAddresses.Data[b].Coin.Equals(exchangeInfo.Data[i].BaseCoin, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        List<AssetNetwork> baseAssetNetworks = [];
                                        for (int c = 0; c < contractAddresses.Data[b].Chains.Length; c++)
                                        {
                                            if (bool.Parse(contractAddresses.Data[b].Chains[c].Rechargeable) || bool.Parse(contractAddresses.Data[b].Chains[c].Withdrawable))
                                            {
                                                AssetNetwork assetNetwork = new()
                                                {
                                                    NetworkName = contractAddresses.Data[b].Chains[c].Chain,
                                                    DepositEnable = bool.Parse(contractAddresses.Data[b].Chains[c].Rechargeable),
                                                    WithdrawEnable = bool.Parse(contractAddresses.Data[b].Chains[c].Withdrawable),
                                                    DepositUrl = $"https://www.bitget.com/asset/recharge?coinId={contractAddresses.Data[b].CoinId}",
                                                    WithdrawUrl = $"https://www.bitget.com/asset/withdraw?coinId={contractAddresses.Data[b].CoinId}"
                                                };

                                                //Withraw fee
                                                if (double.TryParse(contractAddresses.Data[b].Chains[c].WithdrawFee, out double withdrawFee))
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
                                                if (double.TryParse(contractAddresses.Data[b].Chains[c].ExtraWithdrawFee, out double extraWithdrawFee))
                                                {
                                                    assetNetwork.TransferTax = extraWithdrawFee;
                                                }

                                                if (!String.IsNullOrWhiteSpace(contractAddresses.Data[b].Chains[c].ContractAddress))
                                                {
                                                    assetNetwork.Address = contractAddresses.Data[b].Chains[c].ContractAddress;
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

                await File.WriteAllTextAsync(Path.Combine(env.ContentRootPath, "Cache/bitget.json"), JsonSerializer.Serialize(pairs, Helper.serializeOptions));
                
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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/v2/spot/market/orderbook?symbol={baseAsset.ToUpper()}{quoteAsset.ToUpper()}&limit=100").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateBitgetOrderBook);
                string clientName = ClientService.RotatingProxyClient("bitget");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                BitgetOrderBook orderBook = JsonSerializer.Deserialize<BitgetOrderBook>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

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