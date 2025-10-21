using CryptoOculus.Models;
using System.Net;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class KrakenService(IHttpClientFactory httpClientFactory, ILogger<KrakenService> logger, ApiKeysService apiKeys, IWebHostEnvironment env) : IExchange, IDnsUpdate
    {
        public int ExchangeId { get; } = 21;
        public string ExchangeName { get; } = "Kraken Pro";
        public string[] Hosts { get; } = ["api.kraken.com", "iapi.kraken.com"];
        public string[] Ips { get; set; } = [];

        private void ValidateKrakenExchangeInfo(HttpRequestMessage request)
        {
            KrakenExchangeInfo model = ClientService.Deserialize<KrakenExchangeInfo>(request);

            if (model.Result is null || model.Error.Length > 0)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateKrakenDepositMethods(HttpRequestMessage request)
        {
            KrakenDepositMethods model = ClientService.Deserialize<KrakenDepositMethods>(request);

            if (model.Result is null || model.Errors?.Length > 0)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateKrakenWithdrawalMethods(HttpRequestMessage request)
        {
            KrakenWithdrawalMethods model = ClientService.Deserialize<KrakenWithdrawalMethods>(request);

            if (model.Result is null || model.Errors?.Length > 0)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateKrakenPrices(HttpRequestMessage request)
        {
            KrakenPrices model = ClientService.Deserialize<KrakenPrices>(request);

            if (model.Result is null || model.Error.Length > 0)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateKrakenOrderBook(HttpRequestMessage request)
        {
            KrakenOrderBook model = ClientService.Deserialize<KrakenOrderBook>(request);

            if (model.Result is null || model.Error.Length > 0)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }

        public async Task<Pair[]> GetPairs()
        {
            //Getting Current exchange trading rules and symbol information (Exchange info)
            async Task<KrakenExchangeInfo> ExInfo()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/0/public/AssetPairs").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateKrakenExchangeInfo);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<KrakenExchangeInfo>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query deposit details
            async Task<KrakenDepositMethods> DepositMethods()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[1]}/api/internal/deposits/methods").WithVersion();
                request.Headers.Host = Hosts[1];
                request.Headers.Add("cookie", new Cookie("SESSID", apiKeys.GetSingle("KrakenSessId")).ToString());
                request.Headers.Referrer = new Uri("https://pro.kraken.com/");
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateKrakenDepositMethods);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<KrakenDepositMethods>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query withdraw details
            async Task<KrakenWithdrawalMethods> WithdrawalMethods()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[1]}/api/internal/withdrawals/methods").WithVersion();
                request.Headers.Host = Hosts[1];
                request.Headers.Add("cookie", new Cookie("SESSID", apiKeys.GetSingle("KrakenSessId")).ToString());
                request.Headers.Referrer = new Uri("https://pro.kraken.com/");
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateKrakenWithdrawalMethods);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<KrakenWithdrawalMethods>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currect prices of pairs
            async Task<KrakenPrices> Prices()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/0/public/Ticker").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateKrakenPrices);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<KrakenPrices>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            try
            {
                Task<KrakenExchangeInfo> exInfoTask = ExInfo();
                Task<KrakenDepositMethods> depositMethodsTask = DepositMethods();
                Task<KrakenWithdrawalMethods> withdrawalMethodsTask = WithdrawalMethods();
                Task<KrakenPrices> pricesTask = Prices();

                await Task.WhenAll([exInfoTask, depositMethodsTask, withdrawalMethodsTask, pricesTask]);

                KrakenExchangeInfo exchangeInfo = await exInfoTask;
                KrakenDepositMethods depositMethods = await depositMethodsTask;
                KrakenWithdrawalMethods withdrawalMethods = await withdrawalMethodsTask;
                KrakenPrices prices = await pricesTask;

                List<Pair> pairs = [];

                if (exchangeInfo.Result is not null)
                {
                    foreach (var result in exchangeInfo.Result)
                    {
                        string quoteAsset = result.Value.Altname[result.Value.Base.Length..];

                        if (quoteAsset.Equals("USDT", StringComparison.CurrentCultureIgnoreCase) || quoteAsset.Equals("USDC", StringComparison.CurrentCultureIgnoreCase) || quoteAsset.Equals("TUSD", StringComparison.CurrentCultureIgnoreCase) || quoteAsset.Equals("USD", StringComparison.CurrentCultureIgnoreCase))
                        {
                            //Adding basic info of pair
                            Pair pair = new()
                            {
                                ExchangeId = ExchangeId,
                                ExchangeName = ExchangeName,
                                BaseAsset = result.Value.Base,
                                QuoteAsset = quoteAsset
                            };

                            //adding price of pair
                            foreach (var price in prices.Result!)
                            {
                                if (price.Key.Equals(result.Value.Altname, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    if (double.TryParse(price.Value.A[0], out double askPrice))
                                    {
                                        pair.AskPrice = askPrice;
                                    }

                                    if (double.TryParse(price.Value.B[0], out double bidPrice))
                                    {
                                        pair.BidPrice = bidPrice;
                                    }

                                    break;
                                }
                            }

                            //adding supported networks of base asset
                            List<AssetNetwork> baseAssetNetworks = [];

                            List<string?> addedNetworkIds = [];

                            for (int i = 0; i < depositMethods.Result!.Length; i++)
                            {
                                bool isExists = false;

                                //Checking added network ids
                                for (int a = 0; a < addedNetworkIds.Count; a++)
                                {
                                    if (depositMethods.Result[i].NetworkId is not null && depositMethods.Result[i].NetworkId!.Equals(addedNetworkIds[a], StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        isExists = true;
                                        break;
                                    }
                                }

                                if (!isExists && depositMethods.Result[i].DepositNetworkInfo is not null && depositMethods.Result[i].Asset.Equals(pair.BaseAsset, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    baseAssetNetworks.Add(new AssetNetwork()
                                    {
                                        NetworkName = depositMethods.Result[i].DepositNetworkInfo!.Network,
                                        DepositEnable = true
                                    });

                                    addedNetworkIds.Add(depositMethods.Result[i].NetworkId);
                                }
                            }

                            for (int i = 0; i < withdrawalMethods.Result!.Length; i++)
                            {
                                bool isExists = false;

                                for (int a = 0; a < baseAssetNetworks.Count; a++)
                                {
                                    if (withdrawalMethods.Result[i].Asset.Equals(pair.BaseAsset, StringComparison.CurrentCultureIgnoreCase) &&
                                        ((withdrawalMethods.Result[i].WithdrawalNetworkInfo is not null && withdrawalMethods.Result[i].WithdrawalNetworkInfo!.Network.Equals(baseAssetNetworks[a].NetworkName, StringComparison.CurrentCultureIgnoreCase)) ||
                                        (withdrawalMethods.Result[i].NetworkId is not null && withdrawalMethods.Result[i].NetworkId!.Equals(addedNetworkIds[a], StringComparison.CurrentCultureIgnoreCase))))
                                    {
                                        baseAssetNetworks[a].WithdrawEnable = true;

                                        if (double.TryParse(withdrawalMethods.Result[i].Fee, out double withdrawFee))
                                        {
                                            baseAssetNetworks[a].WithdrawFee = withdrawFee;
                                        }

                                        isExists = true;
                                        break;
                                    }
                                }

                                //Checking added network ids
                                if (!isExists)
                                {
                                    for (int a = 0; a < addedNetworkIds.Count; a++)
                                    {
                                        if (withdrawalMethods.Result[i].NetworkId is not null && withdrawalMethods.Result[i].NetworkId!.Equals(addedNetworkIds[a], StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            isExists = true;
                                            break;
                                        }
                                    }
                                }

                                if (!isExists && withdrawalMethods.Result[i].Asset.Equals(pair.BaseAsset, StringComparison.CurrentCultureIgnoreCase) && withdrawalMethods.Result[i].WithdrawalNetworkInfo is not null)
                                {
                                    baseAssetNetworks.Add(new AssetNetwork()
                                    {
                                        NetworkName = withdrawalMethods.Result[i].WithdrawalNetworkInfo!.Network,
                                        WithdrawEnable = true,
                                        WithdrawFee = double.TryParse(withdrawalMethods.Result[i].Fee, out double withdrawFee) ? withdrawFee : null
                                    });
                                }
                            }

                            pair.BaseAssetNetworks = [.. baseAssetNetworks];

                            if ((pair.BidPrice != 0 || pair.AskPrice != 0) && pair.BaseAssetNetworks.Length > 0)
                            {
                                pairs.Add(pair);
                            }
                        }
                    }
                }

                await File.WriteAllTextAsync(Path.Combine(env.ContentRootPath, "Cache/Kraken/firstStepPairs.json"), JsonSerializer.Serialize(pairs, Helper.serializeOptions));

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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/0/public/Depth?pair={baseAsset.ToUpper()}{quoteAsset.ToUpper()}&count=100").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateKrakenOrderBook);
                string clientName = ClientService.RotatingProxyClient("kraken");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                KrakenOrderBook orderBook = JsonSerializer.Deserialize<KrakenOrderBook>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                foreach (var result in orderBook.Result!)
                {
                    if (askOrBid && result.Value.Asks is not null)
                    {
                        List<double[]>? asks = [];

                        for (int a = 0; a < result.Value.Asks.Length; a++)
                        {
                            asks.Add([Convert.ToDouble(result.Value.Asks[a][0].ToString()), Convert.ToDouble(result.Value.Asks[a][1].ToString())]);
                        }

                        return [.. asks];
                    }

                    else if (!askOrBid && result.Value.Bids is not null)
                    {
                        List<double[]>? bids = [];

                        for (int a = 0; a < result.Value.Bids.Length; a++)
                        {
                            bids.Add([Convert.ToDouble(result.Value.Bids[a][0].ToString()), Convert.ToDouble(result.Value.Bids[a][1].ToString())]);
                        }

                        return [.. bids];
                    }
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
