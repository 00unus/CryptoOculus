using CryptoOculus.Models;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class HtxService(IHttpClientFactory httpClientFactory, ILogger<HtxService> logger, IWebHostEnvironment env) : IExchange, IDnsUpdate
    {
        public int ExchangeId { get; } = 8;
        public string ExchangeName { get; } = "HTX";
        public string[] Hosts { get; } = ["api.huobi.pro"];
        public string[] Ips { get; set; } = [];

        private void ValidateHtxExchangeInfo(HttpRequestMessage request)
        {
            HtxExchangeInfo model = ClientService.Deserialize<HtxExchangeInfo>(request);

            if (model.Data is null || model.Status != "ok")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateHtxContractAddresses(HttpRequestMessage request)
        {
            HtxContractAddresses model = ClientService.Deserialize<HtxContractAddresses>(request);

            if (model.Data is null || model.Code != 200)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateHtxPrices(HttpRequestMessage request)
        {
            HtxPrices model = ClientService.Deserialize<HtxPrices>(request);

            if (model.Data is null || model.Status != "ok")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateHtxOrderBook(HttpRequestMessage request)
        {
            HtxOrderBook model = ClientService.Deserialize<HtxOrderBook>(request);

            if (model.Tick is null || model.Status != "ok")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }

        public async Task<Pair[]> GetPairs()
        {
            //Getting Current exchange trading rules and symbol information (Exchange info)
            async Task<HtxExchangeInfo> ExInfo()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v1/common/symbols").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateHtxExchangeInfo);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<HtxExchangeInfo>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currency details and the smart contract address
            async Task<HtxContractAddresses> Contract()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v2/reference/currencies").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateHtxContractAddresses);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<HtxContractAddresses>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currect prices of pairs
            async Task<HtxPrices> Prices()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/market/tickers").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateHtxPrices);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<HtxPrices>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            try
            {
                Task<HtxExchangeInfo> exInfoTask = ExInfo();
                Task<HtxContractAddresses> contractTask = Contract();
                Task<HtxPrices> pricesTask = Prices();

                await Task.WhenAll([exInfoTask, contractTask, pricesTask]);

                HtxExchangeInfo exchangeInfo = await exInfoTask;
                HtxContractAddresses contractAddresses = await contractTask;
                HtxPrices prices = await pricesTask;

                List<Pair> pairs = [];

                if (exchangeInfo.Data is not null)
                {
                    for (int i = 0; i < exchangeInfo.Data.Length; i++)
                    {
                        if (exchangeInfo.Data[i].State == "online" &&
                           (exchangeInfo.Data[i].Quotecurrency.Equals("USDT", StringComparison.CurrentCultureIgnoreCase) || exchangeInfo.Data[i].Quotecurrency.Equals("USDC", StringComparison.CurrentCultureIgnoreCase) || exchangeInfo.Data[i].Quotecurrency.Equals("TUSD", StringComparison.CurrentCultureIgnoreCase)))
                        {
                            //Adding basic info of pair
                            Pair pair = new()
                            {
                                ExchangeId = ExchangeId,
                                ExchangeName = ExchangeName,
                                BaseAsset = exchangeInfo.Data[i].Basecurrency.ToUpper(),
                                QuoteAsset = exchangeInfo.Data[i].Quotecurrency.ToUpper()
                            };

                            //adding price of pair
                            if (prices.Data is not null)
                            {
                                for (int a = 0; a < prices.Data.Length; a++)
                                {
                                    if (prices.Data[a].Symbol.Equals(exchangeInfo.Data[i].Symbol, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        pair.AskPrice = prices.Data[a].Ask;
                                        pair.BidPrice = prices.Data[a].Bid;

                                        break;
                                    }
                                }
                            }

                            //adding supported networks of base asset
                            if (contractAddresses.Data is not null)
                            {
                                for (int a = 0; a < contractAddresses.Data.Length; a++)
                                {
                                    if (contractAddresses.Data[a].Currency.Equals(exchangeInfo.Data[i].Basecurrency, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        List<AssetNetwork> baseAssetNetworks = [];

                                        for (int b = 0; b < contractAddresses.Data[a].Chains.Length; b++)
                                        {
                                            if (contractAddresses.Data[a].Chains[b].DepositStatus == "allowed" || contractAddresses.Data[a].Chains[b].WithdrawStatus == "allowed")
                                            {
                                                AssetNetwork assetNetwork = new()
                                                {
                                                    NetworkName = contractAddresses.Data[a].Chains[b].DisplayName,
                                                    DepositEnable = contractAddresses.Data[a].Chains[b].DepositStatus == "allowed",
                                                    WithdrawEnable = contractAddresses.Data[a].Chains[b].WithdrawStatus == "allowed",
                                                    TransferTax = 0
                                                };

                                                if (double.TryParse(contractAddresses.Data[a].Chains[b].TransactFeeWithdraw, out double transactFeeWithdraw))
                                                {
                                                    if (pair.AskPrice != 0)
                                                    {
                                                        assetNetwork.WithdrawFee = transactFeeWithdraw * pair.AskPrice;
                                                    }

                                                    else if (pair.BidPrice != 0)
                                                    {
                                                        assetNetwork.WithdrawFee = transactFeeWithdraw * pair.BidPrice;
                                                    }
                                                }

                                                if (double.TryParse(contractAddresses.Data[a].Chains[b].TransactFeeRateWithdraw, out double transactFeeRateWithdraw))
                                                {
                                                    assetNetwork.TransferTax = transactFeeRateWithdraw;
                                                }

                                                if (!String.IsNullOrWhiteSpace(contractAddresses.Data[a].Chains[b].ContractAddress))
                                                {
                                                    assetNetwork.Address = contractAddresses.Data[a].Chains[b].ContractAddress;
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

                using StreamWriter sw = new(Path.Combine(env.ContentRootPath, "Cache/Htx/firstStepPairs.json"));
                sw.Write(JsonSerializer.Serialize(pairs, Helper.serializeOptions));

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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/market/depth?symbol={baseAsset.ToLower()}{quoteAsset.ToLower()}&depth=20&type=step0").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateHtxOrderBook);
                string clientName = ClientService.RotatingProxyClient("htx");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                HtxOrderBook orderBook = JsonSerializer.Deserialize<HtxOrderBook>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                if (askOrBid && orderBook.Tick is not null)
                {
                    return orderBook.Tick.Asks;
                }

                else if (!askOrBid && orderBook.Tick is not null)
                {
                    return orderBook.Tick.Bids;
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
