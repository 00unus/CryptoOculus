using CryptoOculus.Models;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class LocalCexCompareService(IOptionsMonitor<Dictionary<string, SpreadsBlacklistItem>> spreadsBlacklist, IOptionsMonitor<Dictionary<string, CoinsBlacklistItem>> coinsBlacklist, IWebHostEnvironment env, DataService dataService)
    {
        public bool IsInBlackList(string id, int buyExchangeId, int sellExchangeId, Pair buyExchangePair, Pair sellExchangePair)
        {
            if (spreadsBlacklist.CurrentValue.TryGetValue(id, out _) ||
                coinsBlacklist.CurrentValue.TryGetValue($"{buyExchangePair.BaseAsset}%||%ALL", out _) ||
                coinsBlacklist.CurrentValue.TryGetValue($"{sellExchangePair.BaseAsset}%||%ALL", out _) ||
                coinsBlacklist.CurrentValue.TryGetValue($"{buyExchangePair.BaseAsset}%||%{buyExchangeId}", out _) ||
                coinsBlacklist.CurrentValue.TryGetValue($"{sellExchangePair.BaseAsset}%||%{sellExchangeId}", out _))
            {
                return true;
            }

            return false;
        }

        public async Task<List<Spread>> Compare(Pair[][] pairs, double percent)
        {
            ConcurrentBag<Spread> results = [];

            Parallel.For(0, pairs.Length, TaskC);

            //for (int i = 0; i < pairs.Length; i++) //First Exchange
            void TaskC(int i)
            {
                for (int a = i + 1; a < pairs.Length; a++) //Second Exchange
                {
                    for (int b = 0; b < pairs[i].Length; b++) //First Exchange Pairs
                    {
                        for (int c = 0; c < pairs[a].Length; c++) //Second Exchange Pairs
                        {
                            //Find identical pairs
                            if (pairs[i][b].BaseAsset == pairs[a][c].BaseAsset)
                            {
                                //Calculating spread
                                double spread = (pairs[i][b].BidPrice / pairs[a][c].AskPrice) - 1;

                                if (spread >= percent && pairs[i][b].BidPrice != 0 && pairs[a][c].AskPrice != 0)
                                {
                                    AssetNetwork[][]? networks = CompareNetworks([.. pairs[a][c].BaseAssetNetworks], [.. pairs[i][b].BaseAssetNetworks]);

                                    if (networks is not null)
                                    {
                                        string id = $"{pairs[a][c].ExchangeId}%||%{pairs[i][b].ExchangeId}%||%{pairs[a][c].BaseAsset}%||%{pairs[a][c].QuoteAsset}%||%{pairs[i][b].BaseAsset}%||%{pairs[i][b].QuoteAsset}";

                                        if (!IsInBlackList(id, pairs[a][c].ExchangeId, pairs[i][b].ExchangeId, pairs[a][c], pairs[i][b]))
                                        {
                                            Spread result = new()
                                            {
                                                Id = id,
                                                BuyExchangeId = pairs[a][c].ExchangeId,
                                                SellExchangeId = pairs[i][b].ExchangeId,
                                                BuyExchangePair = new(pairs[a][c])
                                                {
                                                    BaseAssetNetworks = networks[0]
                                                },
                                                SellExchangePair = new(pairs[i][b])
                                                {
                                                    BaseAssetNetworks = networks[1]
                                                },
                                                SpreadPercent = spread
                                            };

                                            results.Add(result);
                                        }
                                    }
                                }

                                else
                                {
                                    spread = (pairs[a][c].BidPrice / pairs[i][b].AskPrice) - 1;

                                    if (spread >= percent && pairs[a][c].BidPrice != 0 && pairs[i][b].AskPrice != 0)
                                    {
                                        AssetNetwork[][]? networks = CompareNetworks([.. pairs[i][b].BaseAssetNetworks], [.. pairs[a][c].BaseAssetNetworks]);

                                        if (networks is not null)
                                        {
                                            string id = $"{pairs[i][b].ExchangeId}%||%{pairs[a][c].ExchangeId}%||%{pairs[i][b].BaseAsset}%||%{pairs[i][b].QuoteAsset}%||%{pairs[a][c].BaseAsset}%||%{pairs[a][c].QuoteAsset}";

                                            if (!IsInBlackList(id, pairs[i][b].ExchangeId, pairs[a][c].ExchangeId, pairs[i][b], pairs[a][c]))
                                            {
                                                Spread result = new()
                                                {
                                                    Id = id,
                                                    BuyExchangeId = pairs[i][b].ExchangeId,
                                                    SellExchangeId = pairs[a][c].ExchangeId,
                                                    BuyExchangePair = new(pairs[i][b])
                                                    {
                                                        BaseAssetNetworks = networks[0]
                                                    },
                                                    SellExchangePair = new(pairs[a][c])
                                                    {
                                                        BaseAssetNetworks = networks[1]
                                                    },
                                                    SpreadPercent = spread
                                                };

                                                results.Add(result);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            await File.WriteAllTextAsync(Path.Combine(env.ContentRootPath, "Cache/Results.json"), JsonSerializer.Serialize(results, Helper.serializeOptions));

            return [.. results];
        }

        public AssetNetwork[][]? CompareNetworks(List<AssetNetwork> withdrawNetworks, List<AssetNetwork> depositNetworks)
        {
            //Checking status of networks and adding networks id
            withdrawNetworks = [.. withdrawNetworks.Where(n => n.WithdrawEnable).Select(network => {
                AssetNetwork newNetwork = new(network);

                //Adding network id
                string? networkName = newNetwork.NetworkName;
                if (!String.IsNullOrWhiteSpace(networkName))
                {
                    newNetwork.NetworkId = dataService.GetNetworkId(networkName);
                }
                
                //Normalizing contract address
                string? address = newNetwork.Address;
                if (!String.IsNullOrWhiteSpace(address))
                {
                    newNetwork.Address = address.ToLower().Replace(" ", "");
                }

                return newNetwork;
            })];

            depositNetworks = [.. depositNetworks.Where(n => n.DepositEnable).Select(network => {
                AssetNetwork newNetwork = new(network);
                
                //Adding network id
                string? networkName = newNetwork.NetworkName;
                if (!String.IsNullOrWhiteSpace(networkName))
                {
                    newNetwork.NetworkId = dataService.GetNetworkId(networkName);
                }
                
                //Normalizing contract address
                string? address = newNetwork.Address;
                if (!String.IsNullOrWhiteSpace(address))
                {
                    newNetwork.Address = address.ToLower().Replace(" ", "");
                }

                return newNetwork;
            })];

            /*for (int d = 0; d < withdrawNetworks.Count; d++)
            {
                if (!withdrawNetworks[d].WithdrawEnable)
                {
                    withdrawNetworks.RemoveAt(d);
                    d--;
                }

                else
                {
                    //Adding network id
                    string? networkName = withdrawNetworks[d].NetworkName;
                    if (!String.IsNullOrWhiteSpace(networkName))
                    {
                        withdrawNetworks[d].NetworkId = dataService.GetNetworkId(networkName);
                    }

                    //Normalizing contract address
                    string? address = withdrawNetworks[d].Address;
                    if (!String.IsNullOrWhiteSpace(address))
                    {
                        withdrawNetworks[d].Address = address.ToLower().Replace(" ", "");
                    }
                }
            }
            for (int d = 0; d < depositNetworks.Count; d++)
            {
                if (!depositNetworks[d].DepositEnable)
                {
                    depositNetworks.RemoveAt(d);
                    d--;
                }

                else
                {
                    //Adding network id
                    string? networkName = depositNetworks[d].NetworkName;
                    if (!String.IsNullOrWhiteSpace(networkName))
                    {
                        depositNetworks[d].NetworkId = dataService.GetNetworkId(networkName);
                    }

                    //Normalizing contract address
                    string? address = depositNetworks[d].Address;
                    if (!String.IsNullOrWhiteSpace(address))
                    {
                        depositNetworks[d].Address = address.ToLower().Replace(" ", "");
                    }
                }
            }*/

            List<(AssetNetwork withdraw, AssetNetwork deposit)> matches = [];

            foreach (AssetNetwork withdrawNetwork in withdrawNetworks)
            {
                foreach (AssetNetwork depositNetwork in depositNetworks)
                {
                    bool isAddressMatch = !String.IsNullOrWhiteSpace(withdrawNetwork.Address) && !String.IsNullOrWhiteSpace(depositNetwork.Address) && withdrawNetwork.Address == depositNetwork.Address;
                    bool isNetworkIdMatch = withdrawNetwork.NetworkId == depositNetwork.NetworkId;

                    if (isNetworkIdMatch && ((String.IsNullOrWhiteSpace(withdrawNetwork.Address) && String.IsNullOrWhiteSpace(depositNetwork.Address)) || isAddressMatch))
                    {
                        matches.Add((withdrawNetwork, depositNetwork));
                    }
                }
            }

            if (matches.Count == 0)
            {
                return null;
            }

            matches = [.. matches.OrderBy(m => m.withdraw.WithdrawFee ?? double.MaxValue)];

            /*List<AssetNetwork> compWithdrawNetworks = [];
            List<AssetNetwork> compDepositNetworks = [];

            for (int i = 0; i < withdrawNetworks.Count; i++)
            {
                for (int a = 0; a < depositNetworks.Count; a++)
                {
                        if (!String.IsNullOrWhiteSpace(withdrawNetworks[i].Address) && !String.IsNullOrWhiteSpace(depositNetworks[a].Address))
                    {
                        if ((withdrawNetworks[i].Address == depositNetworks[a].Address) &&
                            ((withdrawNetworks[i].NetworkId.HasValue && depositNetworks[a].NetworkId.HasValue) ||
                            (!withdrawNetworks[i].NetworkId.HasValue && !depositNetworks[a].NetworkId.HasValue)) &&
                              withdrawNetworks[i].NetworkId == depositNetworks[a].NetworkId)
                        {
                            //Trying add by withdraw fee
                            if (withdrawNetworks[i].WithdrawFee.HasValue)
                            {
                                bool isAdded = false;

                                for (int b = 0; b < compWithdrawNetworks.Count; b++)
                                {
                                    if (compWithdrawNetworks[b].WithdrawFee > withdrawNetworks[i].WithdrawFee)
                                    {
                                        compWithdrawNetworks.Insert(b, withdrawNetworks[i]);
                                        compDepositNetworks.Insert(b, depositNetworks[a]);
                                        isAdded = true;
                                        break;
                                    }
                                }

                                if (isAdded)
                                {
                                    break;
                                }
                            }

                            compWithdrawNetworks.Add(withdrawNetworks[i]);
                            compDepositNetworks.Add(depositNetworks[a]);
                            break;
                        }
                    }

                    else
                    {
                        if (((withdrawNetworks[i].NetworkId.HasValue && depositNetworks[a].NetworkId.HasValue) ||
                            (!withdrawNetworks[i].NetworkId.HasValue && !depositNetworks[a].NetworkId.HasValue)) &&
                              withdrawNetworks[i].NetworkId == depositNetworks[a].NetworkId)
                        {
                            //Trying add by withdraw fee
                            if (withdrawNetworks[i].WithdrawFee.HasValue)
                            {
                                bool isAdded = false;

                                for (int b = 0; b < compWithdrawNetworks.Count; b++)
                                {
                                    if (compWithdrawNetworks[b].WithdrawFee > withdrawNetworks[i].WithdrawFee)
                                    {
                                        compWithdrawNetworks.Insert(b, withdrawNetworks[i]);
                                        compDepositNetworks.Insert(b, depositNetworks[a]);
                                        isAdded = true;
                                        break;
                                    }
                                }

                                if (isAdded)
                                {
                                    break;
                                }
                            }

                            compWithdrawNetworks.Add(withdrawNetworks[i]);
                            compDepositNetworks.Add(depositNetworks[a]);
                            break;
                        }
                    }
                }
            }

            if (compWithdrawNetworks.Count > 0 && compDepositNetworks.Count > 0)
            {
                return [[.. compWithdrawNetworks], [.. compDepositNetworks]];
            }*/

            return [[.. matches.Select(m => m.withdraw)], [.. matches.Select(m => m.deposit)]];
        }
    }
}
