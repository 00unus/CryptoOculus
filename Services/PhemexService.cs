using CryptoOculus.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class PhemexService(IHttpClientFactory httpClientFactory, ILogger<PhemexService> logger, ApiKeysService apiKeys, IWebHostEnvironment env) : IExchange, IDnsUpdate
    {
        public int ExchangeId { get; } = 17;
        public string ExchangeName { get; } = "Phemex";
        public string[] Hosts { get; } = ["api.phemex.com"];
        public string[] Ips { get; set; } = [];

        private void ValidatePhemexExchangeInfo(HttpRequestMessage request)
        {
            PhemexExchangeInfo model = ClientService.Deserialize<PhemexExchangeInfo>(request);

            if (model.Data is null || model.Code != 0 || model.Msg != "")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidatePhemexDeposit(HttpRequestMessage request)
        {
            PhemexDeposit model = ClientService.Deserialize<PhemexDeposit>(request);

            if (model.Data is null || model.Code != 0 || model.Msg != "OK")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidatePhemexWithdraw(HttpRequestMessage request)
        {
            PhemexWithdraw model = ClientService.Deserialize<PhemexWithdraw>(request);

            if (model.Data is null || model.Code != 0 || model.Msg != "ok")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidatePhemexPrices(HttpRequestMessage request)
        {
            PhemexPrices model = ClientService.Deserialize<PhemexPrices>(request);

            if (model.Result is null || model.Id != 0 || model.Error != null)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidatePhemexOrderBook(HttpRequestMessage request)
        {
            PhemexOrderBook model = ClientService.Deserialize<PhemexOrderBook>(request);

            if (model.Result is null || model.Id != 0 || model.Error != null)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }

        private async Task<PhemexContractAddresses> ContractAddressesUpdateAsync(PhemexExchangeInfo exchangeInfo)
        {
            List<string> bases = [];

            //Selecting tradable currencies and сhecking for repeats
            for (int i = 0; i < exchangeInfo.Data!.Products.Length; i++)
            {
                if (exchangeInfo.Data.Products[i].Status == "Listed" && exchangeInfo.Data.Products[i].Type == "Spot")
                {
                    bool isExist = false;
                    string? baseCurrency = exchangeInfo.Data.Products[i].BaseCurrency;

                    if (baseCurrency is not null)
                    {
                        for (int a = 0; a < bases.Count; a++)
                        {
                            if (baseCurrency.Equals(bases[a], StringComparison.CurrentCultureIgnoreCase))
                            {
                                isExist = true;
                                break;
                            }
                        }

                        if (!isExist)
                        {
                            bases.Add(baseCurrency);
                        }
                    }
                }
            }

            string[] pApiKeys = apiKeys.GetMultiple("PhemexApiKeys");
            string[] pSecretKeys = apiKeys.GetMultiple("PhemexSecretKeys");

            List<Task> tasks = [];
            Lock locker = new();

            List<PhemexCurrency> currencies = [];
            int apiIndex = 0;

            for (int i = 0; i < bases.Count; i++)
            {
                int buffer = i;
                int apiIndexBuffer = apiIndex;
                apiIndex = (apiIndex + 1) % pApiKeys.Length;

                tasks.Add(GetCurrencyAsync(buffer, apiIndexBuffer));
            }

            async Task GetCurrencyAsync(int index, int apiIndex)
            {
                PhemexDeposit? phemexDeposit = null;
                PhemexWithdraw? phemexWithdraw = null;

                async Task DepositStatus()
                {
                    using HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(pSecretKeys[apiIndex]));
                    long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"/phemex-deposit/wallets/api/chainCfgcurrency={bases[index].ToUpper()}{now}"));

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/phemex-deposit/wallets/api/chainCfg?currency={bases[index].ToUpper()}").WithVersion();

                    request.Headers.Host = Hosts[0];
                    request.Headers.Add("x-phemex-access-token", pApiKeys[apiIndex]);
                    request.Headers.Add("x-phemex-request-expiry", now.ToString());
                    request.Headers.Add("x-phemex-request-signature", Convert.ToHexStringLower(computedHash));
                    request.Options.Set(HttpOptionKeys.KeyedLimitRuleName, "PhemexOthersIpGroup");
                    request.Options.Set(HttpOptionKeys.KeyedLimitRuleKey, pApiKeys[apiIndex]);
                    request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidatePhemexDeposit);
                    string clientName = ClientService.RotatingProxyClient("phemex");
                    request.Options.Set(HttpOptionKeys.ClientName, clientName);

                    HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                    phemexDeposit = JsonSerializer.Deserialize<PhemexDeposit>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions);
                }

                async Task WithdrawStatus()
                {
                    using HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(pSecretKeys[apiIndex]));
                    long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"/phemex-withdraw/wallets/api/asset/infocurrency={bases[index].ToUpper()}{now}"));

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/phemex-withdraw/wallets/api/asset/info?currency={bases[index].ToUpper()}").WithVersion();

                    request.Headers.Host = Hosts[0];
                    request.Headers.Add("x-phemex-access-token", pApiKeys[apiIndex]);
                    request.Headers.Add("x-phemex-request-expiry", now.ToString());
                    request.Headers.Add("x-phemex-request-signature", Convert.ToHexStringLower(computedHash));
                    request.Options.Set(HttpOptionKeys.KeyedLimitRuleName, "PhemexOthersIpGroup");
                    request.Options.Set(HttpOptionKeys.KeyedLimitRuleKey, pApiKeys[apiIndex]);
                    request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidatePhemexWithdraw);
                    string clientName = ClientService.RotatingProxyClient("phemex");
                    request.Options.Set(HttpOptionKeys.ClientName, clientName);

                    HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                    phemexWithdraw = JsonSerializer.Deserialize<PhemexWithdraw>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions);
                }

                try
                {
                    await Task.WhenAll([DepositStatus(), WithdrawStatus()]);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to fully retrieve information about the coin!");
                }

                List<AssetNetwork> assetNetworkList = [];

                if (phemexDeposit is not null && phemexDeposit.Data is not null)
                {
                    for (int i = 0; i < phemexDeposit.Data.Length; i++)
                    {
                        AssetNetwork assetNetwork = new()
                        {
                            NetworkName = phemexDeposit.Data[i].ChainName,
                            DepositEnable = phemexDeposit.Data[i].Status == "Active"
                        };

                        if (!String.IsNullOrWhiteSpace(phemexDeposit.Data[i].ContractAddress))
                        {
                            assetNetwork.Address = phemexDeposit.Data[i].ContractAddress;
                        }

                        assetNetworkList.Add(assetNetwork);
                    }
                }

                if (phemexWithdraw is not null && phemexWithdraw.Data is not null && phemexWithdraw.Data.ChainInfos is not null)
                {
                    for (int i = 0; i < phemexWithdraw.Data.ChainInfos.Length; i++)
                    {
                        bool isExists = false;

                        for (int a = 0; a < assetNetworkList.Count; a++)
                        {
                            if (phemexWithdraw.Data.ChainInfos[i].ChainName.Equals(assetNetworkList[a].NetworkName, StringComparison.CurrentCultureIgnoreCase))
                            {
                                assetNetworkList[a].WithdrawEnable = phemexWithdraw.Data.ChainInfos[i].Status == "Active";

                                if (double.TryParse(phemexWithdraw.Data.ChainInfos[i].WithdrawFeeRv, out double withdrawFee))
                                {
                                    assetNetworkList[a].WithdrawFee = withdrawFee;
                                }

                                if (!String.IsNullOrWhiteSpace(phemexWithdraw.Data.ChainInfos[i].ContractAddress) && String.IsNullOrWhiteSpace(assetNetworkList[a].Address))
                                {
                                    assetNetworkList[a].Address = phemexWithdraw.Data.ChainInfos[i].ContractAddress;
                                }

                                isExists = true;
                                break;
                            }
                        }

                        if (!isExists)
                        {
                            AssetNetwork assetNetwork = new()
                            {
                                NetworkName = phemexWithdraw.Data.ChainInfos[i].ChainName,
                                WithdrawEnable = phemexWithdraw.Data.ChainInfos[i].Status == "Active"
                            };

                            if (double.TryParse(phemexWithdraw.Data.ChainInfos[i].WithdrawFeeRv, out double withdrawFee))
                            {
                                assetNetwork.WithdrawFee = withdrawFee;
                            }

                            if (!String.IsNullOrWhiteSpace(phemexWithdraw.Data.ChainInfos[i].ContractAddress))
                            {
                                assetNetwork.Address = phemexWithdraw.Data.ChainInfos[i].ContractAddress;
                            }

                            assetNetworkList.Add(assetNetwork);
                        }
                    }
                }

                if (assetNetworkList.Count > 0)
                {
                    PhemexCurrency currency = new()
                    {
                        Currency = bases[index].ToUpper(),
                        Networks = [.. assetNetworkList]
                    };

                    lock (locker)
                    {
                        currencies.Add(currency);
                    }
                }
            }

            await Task.WhenAll(tasks);

            PhemexContractAddresses contractAddresses = new()
            {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Currencies = [.. currencies]
            };

            await File.WriteAllTextAsync(Path.Combine(env.ContentRootPath, "Cache/Phemex/contractAddresses.json"), JsonSerializer.Serialize(contractAddresses, Helper.serializeOptions));

            return contractAddresses;
        }

        public async Task<Pair[]> GetPairs()
        {
            //Getting Current exchange trading rules and symbol information (Exchange info)
            async Task<(PhemexExchangeInfo, PhemexContractAddresses)> InfoAndContract()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/public/products").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidatePhemexExchangeInfo);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                PhemexExchangeInfo exchangeInfo = JsonSerializer.Deserialize<PhemexExchangeInfo>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                //Query currency details and the smart contract address
                string path = Path.Combine(env.ContentRootPath, "Cache/Phemex/contractAddresses.json");

                if (File.Exists(path))
                {
                    PhemexContractAddresses? contractAddresses = JsonSerializer.Deserialize<PhemexContractAddresses>(await File.ReadAllTextAsync(path), Helper.deserializeOptions);

                    if (contractAddresses is not null && TimeSpan.FromSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - contractAddresses.Timestamp).TotalHours < 1)
                    {
                        return (exchangeInfo, contractAddresses);
                    }
                }

                logger.LogInformation("Phemex: Calling ContractAddressesUpdate");
                return (exchangeInfo, await ContractAddressesUpdateAsync(exchangeInfo));
            }

            //Query currect prices of pairs
            async Task<PhemexPrices> Prices()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/md/spot/ticker/24hr/all").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidatePhemexPrices);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<PhemexPrices>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            try
            {
                Task<(PhemexExchangeInfo, PhemexContractAddresses)> infoAndContractTask = InfoAndContract();
                Task<PhemexPrices> pricesTask = Prices();

                await Task.WhenAll([infoAndContractTask, pricesTask]);

                (PhemexExchangeInfo exchangeInfo, PhemexContractAddresses contractAddresses) = await infoAndContractTask;
                PhemexPrices prices = await pricesTask;

                List<Pair> pairs = [];

                if (exchangeInfo.Data is not null)
                {
                    for (int i = 0; i < exchangeInfo.Data.Products.Length; i++)
                    {
                        if (exchangeInfo.Data.Products[i].Status == "Listed" && exchangeInfo.Data.Products[i].Type == "Spot" &&
                           (exchangeInfo.Data.Products[i].QuoteCurrency == "USDT" || exchangeInfo.Data.Products[i].QuoteCurrency == "USDC" || exchangeInfo.Data.Products[i].QuoteCurrency == "TUSD"))
                        {
                            //Adding basic info of pair
                            string? baseAsset = exchangeInfo.Data.Products[i].BaseCurrency;
                            if (baseAsset is null)
                            {
                                break;
                            }

                            Pair pair = new()
                            {
                                ExchangeId = ExchangeId,
                                ExchangeName = ExchangeName,
                                BaseAsset = baseAsset.ToUpper(),
                                QuoteAsset = exchangeInfo.Data.Products[i].QuoteCurrency.ToUpper()
                            };

                            //adding price of pair
                            if (prices.Result is not null)
                            {
                                for (int a = 0; a < prices.Result.Length; a++)
                                {
                                    if (prices.Result[a].Symbol.Equals(exchangeInfo.Data.Products[i].Symbol, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        pair.AskPrice = prices.Result[a].AskEp / 100000000;
                                        pair.BidPrice = prices.Result[a].BidEp / 100000000;

                                        break;
                                    }
                                }
                            }

                            //adding supported networks of base asset
                            for (int b = 0; b < contractAddresses.Currencies.Length; b++)
                            {
                                if (contractAddresses.Currencies[b].Currency.Equals(exchangeInfo.Data.Products[i].BaseCurrency, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    List<AssetNetwork> baseAssetNetworks = [];

                                    for (int c = 0; c < contractAddresses.Currencies[b].Networks.Length; c++)
                                    {
                                        if (contractAddresses.Currencies[b].Networks[c].DepositEnable || contractAddresses.Currencies[b].Networks[c].WithdrawEnable)
                                        {
                                            AssetNetwork assetNetwork = new()
                                            {
                                                NetworkName = contractAddresses.Currencies[b].Networks[c].NetworkName,
                                                DepositEnable = contractAddresses.Currencies[b].Networks[c].DepositEnable,
                                                WithdrawEnable = contractAddresses.Currencies[b].Networks[c].WithdrawEnable
                                            };

                                            if (!String.IsNullOrWhiteSpace(contractAddresses.Currencies[b].Networks[c].Address))
                                            {
                                                assetNetwork.Address = contractAddresses.Currencies[b].Networks[c].Address;
                                            }

                                            if (contractAddresses.Currencies[b].Networks[c].WithdrawFee is not null)
                                            {
                                                if (pair.AskPrice != 0)
                                                {
                                                    assetNetwork.WithdrawFee = pair.AskPrice * contractAddresses.Currencies[b].Networks[c].WithdrawFee;
                                                }

                                                else if (pair.BidPrice != 0)
                                                {
                                                    assetNetwork.WithdrawFee = pair.BidPrice * contractAddresses.Currencies[b].Networks[c].WithdrawFee;
                                                }
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

                            if ((pair.BidPrice != 0 || pair.AskPrice != 0) && pair.BaseAssetNetworks.Length > 0)
                            {
                                pairs.Add(pair);
                            }
                        }
                    }
                }

                await File.WriteAllTextAsync(Path.Combine(env.ContentRootPath, "Cache/Phemex/firstStepPairs.json"), JsonSerializer.Serialize(pairs, Helper.serializeOptions));

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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/md/orderbook?symbol=s{baseAsset.ToUpper()}{quoteAsset.ToUpper()}").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.LimitRuleName, "PhemexIpLimit");
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidatePhemexOrderBook);
                string clientName = ClientService.RotatingProxyClient("phemex");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                PhemexOrderBook orderBook = JsonSerializer.Deserialize<PhemexOrderBook>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                if (askOrBid && orderBook.Result is not null && orderBook.Result.Book.Asks is not null)
                {
                    List<double[]> returnOrderBook = [];

                    for (int a = 0; a < orderBook.Result.Book.Asks.Length; a++)
                    {
                        returnOrderBook.Add([orderBook.Result.Book.Asks[a][0] / 100000000, orderBook.Result.Book.Asks[a][1] / 100000000]);
                    }

                    return [.. returnOrderBook];
                }

                else if (!askOrBid && orderBook.Result is not null && orderBook.Result.Book.Bids is not null)
                {
                    List<double[]> returnOrderBook = [];

                    for (int a = 0; a < orderBook.Result.Book.Bids.Length; a++)
                    {
                        returnOrderBook.Add([orderBook.Result.Book.Bids[a][0] / 100000000, orderBook.Result.Book.Bids[a][1] / 100000000]);
                    }

                    return [.. returnOrderBook];
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
