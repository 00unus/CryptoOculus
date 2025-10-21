using CryptoOculus.Models;
using System.Text;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class TelegramService(IHttpClientFactory httpClientFactory, ILogger<TelegramService> logger, CexsLoadService cexsLoad, ApiKeysService apiKeys, LanguagesService languages, UserService userService, Helper helper, DataService dataService)
    {
        public static string ToJson(params string?[] strings)
        {
            return JsonSerializer.Serialize(strings);
        }
        public static string ToSuperscript(string text)
        {
            return new([.. text.Select(c => c switch
            {
                '0' => '⁰',
                '1' => '¹',
                '2' => '²',
                '3' => '³',
                '4' => '⁴',
                '5' => '⁵',
                '6' => '⁶',
                '7' => '⁷',
                '8' => '⁸',
                '9' => '⁹',
                '+' => '⁺',
                '-' => '⁻',
                '=' => '⁼',
                _ => c
            })]);
        }
        private static string EscapeMarkdownV2(string text)
        {
            return text.Replace("_", "\\_").Replace("*", "\\*").Replace("[", "\\[").Replace("]", "\\]").Replace("(", "\\(").Replace(")", "\\)").Replace("~", "\\~").Replace("`", "\\`").Replace(">", "\\>").Replace("#", "\\#").Replace("+", "\\+").Replace("-", "\\-").Replace("=", "\\=").Replace("|", "\\|").Replace("{", "\\{").Replace("}", "\\}").Replace(".", "\\.").Replace("!", "\\!");
        }
        private string FillSpread(Spread spread, string languageCode)
        {
            string text = languages.GetString(languageCode, "Spread");

            string withdrawalChains = "";
            for (int b = 0; b < spread.BuyExchangePair.BaseAssetNetworks.Length; b++)
            {
                string withdrawalChain = languages.GetString(languageCode, "WithdrawalChain");

                if (spread.BuyExchangePair.BaseAssetNetworks[b].NetworkId.HasValue)
                {
                    withdrawalChain = withdrawalChain.Replace("{network name}", EscapeMarkdownV2($"{dataService.GetNetworkName((int)spread.BuyExchangePair.BaseAssetNetworks[b].NetworkId!)} ({spread.BuyExchangePair.BaseAssetNetworks[b].NetworkName})"));
                }
                else
                {
                    withdrawalChain = withdrawalChain.Replace("{network name}", EscapeMarkdownV2(spread.BuyExchangePair.BaseAssetNetworks[b].NetworkName ?? "UNKNOWN"));
                }

                withdrawalChain = withdrawalChain.Replace("{withdrawal link}", "https://www.cryptooculus.com/");

                if (spread.BuyExchangePair.BaseAssetNetworks[b].WithdrawFee.HasValue)
                {
                    withdrawalChain = withdrawalChain.Replace("{fee}", EscapeMarkdownV2($"{Math.Round((double)spread.BuyExchangePair.BaseAssetNetworks[b].WithdrawFee!, 3)}$"));
                }
                else
                {
                    withdrawalChain = withdrawalChain.Replace("{fee}", "❓");
                }

                if (spread.BuyExchangePair.BaseAssetNetworks[b].TransferTax.HasValue)
                {
                    withdrawalChain = withdrawalChain.Replace("{transfer tax}", EscapeMarkdownV2($"{spread.BuyExchangePair.BaseAssetNetworks[b].TransferTax}%"));
                }
                else
                {
                    withdrawalChain = withdrawalChain.Replace("{transfer tax}", "❓");
                }

                withdrawalChains += withdrawalChain;
            }

            string depositChains = "";
            for (int b = 0; b < spread.SellExchangePair.BaseAssetNetworks.Length; b++)
            {
                string depositChain = languages.GetString(languageCode, "DepositChain");

                if (spread.SellExchangePair.BaseAssetNetworks[b].NetworkId.HasValue)
                {
                    depositChain = depositChain.Replace("{network name}", EscapeMarkdownV2($"{dataService.GetNetworkName((int)spread.SellExchangePair.BaseAssetNetworks[b].NetworkId!)} ({spread.SellExchangePair.BaseAssetNetworks[b].NetworkName})"));
                }
                else
                {
                    depositChain = depositChain.Replace("{network name}", EscapeMarkdownV2(spread.SellExchangePair.BaseAssetNetworks[b].NetworkName ?? "UNKNOWN"));
                }

                depositChain = depositChain.Replace("{deposit link}", "https://www.cryptooculus.com/");

                depositChains += depositChain;
            }

            text = text.Replace("{best profit}", EscapeMarkdownV2(Math.Round(spread.Profit, 2).ToString()));
            text = text.Replace("{best spread}", EscapeMarkdownV2(Math.Round(spread.SpreadPercent * 100, 2).ToString()));
            text = text.Replace("{best amount}", EscapeMarkdownV2(Math.Round(spread.BuyExchangePair.AskAmount, 2).ToString()));
            text = text.Replace("{buy exchange}", EscapeMarkdownV2(spread.BuyExchangePair.ExchangeName));
            text = text.Replace("{buy pair base asset}", EscapeMarkdownV2(spread.BuyExchangePair.BaseAsset));
            text = text.Replace("{buy pair quote asset}", EscapeMarkdownV2(spread.BuyExchangePair.QuoteAsset));
            text = text.Replace("{buy pair url}", helper.GetExchangePairLink(spread.BuyExchangeId, spread.BuyExchangePair.BaseAsset, spread.BuyExchangePair.QuoteAsset));
            text = text.Replace("{buy price}", EscapeMarkdownV2(Math.Round((decimal)spread.BuyExchangePair.AskPrice, ((decimal)spread.BuyExchangePair.AskPriceRange![0]).Scale).ToString()));
            text = text.Replace("{buy price range 1}", EscapeMarkdownV2(((decimal)spread.BuyExchangePair.AskPriceRange![0]).ToString()));
            text = text.Replace("{buy price range 2}", EscapeMarkdownV2(((decimal)spread.BuyExchangePair.AskPriceRange![1]).ToString()));
            text = text.Replace("{buy quantity}", EscapeMarkdownV2(Math.Round(spread.BuyExchangePair.AskQuantity, 4).ToString()));
            text = text.Replace("{buy amount}", EscapeMarkdownV2(Math.Round(spread.BuyExchangePair.AskAmount, 2).ToString()));
            text = text.Replace("{withdrawal chains}", withdrawalChains);

            text = text.Replace("{sell exchange}", EscapeMarkdownV2(spread.SellExchangePair.ExchangeName));
            text = text.Replace("{sell pair base asset}", EscapeMarkdownV2(spread.SellExchangePair.BaseAsset));
            text = text.Replace("{sell pair quote asset}", EscapeMarkdownV2(spread.SellExchangePair.QuoteAsset));
            text = text.Replace("{sell pair url}", helper.GetExchangePairLink(spread.SellExchangeId, spread.SellExchangePair.BaseAsset, spread.SellExchangePair.QuoteAsset));
            text = text.Replace("{sell price}", EscapeMarkdownV2(Math.Round((decimal)spread.SellExchangePair.BidPrice, ((decimal)spread.SellExchangePair.BidPriceRange![0]).Scale).ToString()));
            text = text.Replace("{sell price range 1}", EscapeMarkdownV2(((decimal)spread.SellExchangePair.BidPriceRange![0]).ToString()));
            text = text.Replace("{sell price range 2}", EscapeMarkdownV2(((decimal)spread.SellExchangePair.BidPriceRange![1]).ToString()));
            text = text.Replace("{sell quantity}", EscapeMarkdownV2(Math.Round(spread.SellExchangePair.BidQuantity, 4).ToString()));
            text = text.Replace("{sell amount}", EscapeMarkdownV2(Math.Round(spread.SellExchangePair.BidAmount, 2).ToString()));
            text = text.Replace("{deposit chains}", depositChains);

            text = text.Replace("{time}", TimeToText(languageCode, (int)Math.Round((DateTimeOffset.UtcNow - spread.FindingDate).TotalSeconds)));
            text = text.Replace("{ago}", languages.GetString(languageCode, "Ago"));

            return text;
        }
        private (string text, List<TelegramInlineKeyboardMarkup[]> inlineKeyboardMarkups, int requiredPage)? FillInnerSpreads(OrderedDictionary<string, Spread> spreads, UserSettings userSettings, SubscribeDetails subscribeItem)
        {
            if (subscribeItem.Type == "InnerSpreads" && subscribeItem.InnerSpreadsId is not null && subscribeItem.RequiredPage.HasValue)
            {
                string[] numerialEmoji = ["1️⃣", "2️⃣", "3️⃣", "4️⃣", "5️⃣", "6️⃣", "7️⃣", "8️⃣", "9️⃣", "🔟"];

                int spreadsPerPage = 10;

                string text = languages.GetString(userSettings.LanguageCode, "Spreads");
                text = text.Replace("{spreads count}", spreads.Count.ToString());

                int maxPage = Math.Max(1, (int)Math.Ceiling((double)spreads.Count / spreadsPerPage));
                int requiredPage = Math.Clamp(subscribeItem.RequiredPage.Value, 1, maxPage);
                int start = (requiredPage - 1) * spreadsPerPage;

                List<TelegramInlineKeyboardMarkup> innerTelegramInlineKeyboardMarkups = [];
                StringBuilder spreadsText = new($"*{subscribeItem.InnerSpreadsId} :*\n");

                for (int a = start; a < Math.Min(start + spreadsPerPage, spreads.Count); a++)
                {
                    Spread spread = spreads.ElementAt(a).Value;

                    spreadsText.Append($"{numerialEmoji[a - start]} *{EscapeMarkdownV2(spread.BuyExchangePair.ExchangeName)} \\=\\> {EscapeMarkdownV2(spread.SellExchangePair.ExchangeName)}* ᛁ *{EscapeMarkdownV2(spread.BuyExchangePair.BaseAsset)}/{EscapeMarkdownV2(spread.BuyExchangePair.QuoteAsset)}*\n");
                    spreadsText.Append($"*{EscapeMarkdownV2(Math.Round(spread.Profit, 1).ToString())}$* \\({EscapeMarkdownV2(Math.Round(spread.SpreadPercent * 100, 1).ToString())}%\\) ᛁ *{EscapeMarkdownV2(Math.Round(spread.BuyExchangePair.AskAmount, 1).ToString())}$*\n");
                    spreadsText.Append($"{EscapeMarkdownV2(spread.BuyExchangePair.BaseAssetNetworks[0].NetworkName ?? "UNKNOWN")} \\=\\> {EscapeMarkdownV2(spread.SellExchangePair.BaseAssetNetworks[0].NetworkName ?? "UNKNOWN")} ᛁ _{TimeToText(userSettings.LanguageCode, (int)Math.Round((DateTimeOffset.UtcNow - spread.FindingDate).TotalSeconds))} {languages.GetString(userSettings.LanguageCode, "Ago")}_\n\n");

                    innerTelegramInlineKeyboardMarkups.Add(new() { Text = numerialEmoji[a - start], CallBackData = ToJson($"Spread_{JsonSerializer.Serialize<SubscribeDetails>(new() { Type = "Spread", SpreadId = spreads.ElementAt(a).Key, BackString = $"InnerSpreads_{JsonSerializer.Serialize(subscribeItem)}" })}") });
                }

                text = text.Replace("{spreads}", spreadsText.ToString());

                List<TelegramInlineKeyboardMarkup[]> telegramInlineKeyboardMarkups = innerTelegramInlineKeyboardMarkups.Count > 5 ? [[.. innerTelegramInlineKeyboardMarkups.Take(innerTelegramInlineKeyboardMarkups.Count / 2)], [.. innerTelegramInlineKeyboardMarkups.Skip(innerTelegramInlineKeyboardMarkups.Count / 2)]] : [[.. innerTelegramInlineKeyboardMarkups]];

                telegramInlineKeyboardMarkups.Add([
                    new() { Text = "«", CallBackData = ToJson(requiredPage > 1 ? $"InnerSpreads_{JsonSerializer.Serialize<SubscribeDetails>(new() { Type = "InnerSpreads", RequiredPage = requiredPage - 1, InnerSpreadsId = subscribeItem.InnerSpreadsId, BackString = subscribeItem.BackString })}" : "null") },
                    new() { Text = $"{requiredPage}/{maxPage}", CallBackData = ToJson("null") },
                    new() { Text = "»", CallBackData = ToJson(requiredPage < maxPage ? $"InnerSpreads_{JsonSerializer.Serialize<SubscribeDetails>(new() { Type = "InnerSpreads", RequiredPage = requiredPage + 1, InnerSpreadsId = subscribeItem.InnerSpreadsId, BackString = subscribeItem.BackString })}" : "null") }]);

                return (text, telegramInlineKeyboardMarkups, requiredPage);
            }

            return null;
        }
        private (string text, List<TelegramInlineKeyboardMarkup[]> inlineKeyboardMarkups, int requiredPage)? FillSpreads(OrderedDictionary<string, OrderedDictionary<string, Spread>> innerSpreads, UserSettings userSettings, SubscribeDetails subscribeDetails)
        {
            if (subscribeDetails.Type == "Spreads" && subscribeDetails.RequiredPage.HasValue)
            {
                string[] numerialEmoji = ["1️⃣", "2️⃣", "3️⃣", "4️⃣", "5️⃣", "6️⃣", "7️⃣", "8️⃣", "9️⃣", "🔟"];

                int spreadsPerPage = 10;

                int spreadsCount = 0;
                foreach (var innerSpread in innerSpreads)
                {
                    spreadsCount += innerSpread.Value.Count;
                }

                string text = languages.GetString(userSettings.LanguageCode, "Spreads");
                text = text.Replace("{spreads count}", spreadsCount.ToString());

                int maxPage = Math.Max(1, (int)Math.Ceiling((double)innerSpreads.Count / spreadsPerPage));
                int requiredPage = Math.Clamp(subscribeDetails.RequiredPage.Value, 1, maxPage);
                int start = (requiredPage - 1) * spreadsPerPage;

                List<TelegramInlineKeyboardMarkup> innerTelegramInlineKeyboardMarkups = [];
                StringBuilder spreadsText = new();

                for (int a = start; a < Math.Min(start + spreadsPerPage, innerSpreads.Count); a++)
                {
                    Spread spread = innerSpreads.ElementAt(a).Value.ElementAt(0).Value;

                    spreadsText.Append($"{numerialEmoji[a - start]} *{innerSpreads.ElementAt(a).Key}{ToSuperscript(innerSpreads.ElementAt(a).Value.Count.ToString())} :*\n");
                    spreadsText.Append($"*{EscapeMarkdownV2(spread.BuyExchangePair.ExchangeName)} \\=\\> {EscapeMarkdownV2(spread.SellExchangePair.ExchangeName)}* ᛁ *{EscapeMarkdownV2(spread.BuyExchangePair.BaseAsset)}/{EscapeMarkdownV2(spread.BuyExchangePair.QuoteAsset)}*\n");
                    spreadsText.Append($"*{EscapeMarkdownV2(Math.Round(spread.Profit, 1).ToString())}$* \\({EscapeMarkdownV2(Math.Round(spread.SpreadPercent * 100, 1).ToString())}%\\) ᛁ *{EscapeMarkdownV2(Math.Round(spread.BuyExchangePair.AskAmount, 1).ToString())}$*\n");
                    spreadsText.Append($"{EscapeMarkdownV2(spread.BuyExchangePair.BaseAssetNetworks[0].NetworkName ?? "UNKNOWN")} \\=\\> {EscapeMarkdownV2(spread.SellExchangePair.BaseAssetNetworks[0].NetworkName ?? "UNKNOWN")} ᛁ _{TimeToText(userSettings.LanguageCode, (int)Math.Round((DateTimeOffset.UtcNow - spread.FindingDate).TotalSeconds))} {languages.GetString(userSettings.LanguageCode, "Ago")}_\n\n");

                    innerTelegramInlineKeyboardMarkups.Add(new() { Text = numerialEmoji[a - start], CallBackData = ToJson($"InnerSpreads_{JsonSerializer.Serialize<SubscribeDetails>(new() { Type = "InnerSpreads", RequiredPage = 1, InnerSpreadsId = innerSpreads.ElementAt(a).Key, BackString = $"Spreads_{JsonSerializer.Serialize(subscribeDetails)}" })}") });
                }

                text = text.Replace("{spreads}", spreadsText.ToString());

                List<TelegramInlineKeyboardMarkup[]> telegramInlineKeyboardMarkups = innerTelegramInlineKeyboardMarkups.Count > 5 ? [[.. innerTelegramInlineKeyboardMarkups.Take(innerTelegramInlineKeyboardMarkups.Count / 2)], [.. innerTelegramInlineKeyboardMarkups.Skip(innerTelegramInlineKeyboardMarkups.Count / 2)]] : [[.. innerTelegramInlineKeyboardMarkups]];

                telegramInlineKeyboardMarkups.Add([
                    new() { Text = "«", CallBackData = ToJson(requiredPage > 1 ? $"Spreads_{JsonSerializer.Serialize < SubscribeDetails >(new() { Type = "Spreads", RequiredPage = requiredPage - 1, BackString = subscribeDetails.BackString })}" : "null") },
                    new() { Text = $"{requiredPage}/{maxPage}", CallBackData = ToJson("null") },
                    new() { Text = "»", CallBackData = ToJson(requiredPage < maxPage ? $"Spreads_{JsonSerializer.Serialize < SubscribeDetails >(new() { Type = "Spreads", RequiredPage = requiredPage + 1, BackString = subscribeDetails.BackString })}" : "null") }]);

                return (text, telegramInlineKeyboardMarkups, requiredPage);
            }

            return null;
        }
        private string TimeToText(string languageCode, long seconds)
        {
            double minutes = Math.Round(TimeSpan.FromSeconds(seconds).TotalMinutes);
            double hours = Math.Round(TimeSpan.FromSeconds(seconds).TotalHours);

            if (seconds < 60)
            {
                return $"{EscapeMarkdownV2(seconds.ToString())} {(seconds <= 1 ? languages.GetString(languageCode, "Second") : languages.GetString(languageCode, "Seconds"))}";
            }
            else if (minutes < 60)
            {
                return $"{EscapeMarkdownV2(minutes.ToString())} {(minutes <= 1 ? languages.GetString(languageCode, "Minute") : languages.GetString(languageCode, "Minutes"))}";
            }
            else
            {
                return $"{EscapeMarkdownV2(hours.ToString())} {(hours <= 1 ? languages.GetString(languageCode, "Hour") : languages.GetString(languageCode, "Hours"))}";
            }
        }

        public async void SendNewSpreads(string[] ids, CancellationToken cancellationToken)
        {
            UserSettings[] usersSettings = await userService.GetUsersSettings();
            List<Task> tasks = [];

            async Task Send(UserSettings userSettings)
            {
                await Task.Delay(TimeSpan.FromSeconds(userSettings.NewSpreadsDelay), cancellationToken);

                foreach (string id in ids)
                {
                    OrderedDictionary<string, Spread> spreads = CexCompareService.GetSpreads(
                        id, userSettings.Profit, userSettings.Amount,
                        JsonSerializer.Deserialize<Dictionary<string, SpreadsBlacklistItem>>(userSettings.SpreadsBlacklist) ?? [],
                        JsonSerializer.Deserialize<Dictionary<string, CoinsBlacklistItem>>(userSettings.CoinsBlacklist) ?? [],
                        JsonSerializer.Deserialize<HashSet<int>>(userSettings.BuyExBlacklist) ?? [],
                        JsonSerializer.Deserialize<HashSet<int>>(userSettings.SellExBlacklist) ?? []);

                    if (spreads.Count > 0 && FillInnerSpreads(spreads, userSettings, new() { Type = "InnerSpreads", RequiredPage = 1, InnerSpreadsId = id }) is (string text, List<TelegramInlineKeyboardMarkup[]> inlineKeyboardMarkups, int requiredPage))
                    {
                        inlineKeyboardMarkups.Add([new() { Text = languages.GetString(userSettings.LanguageCode, "SubscribeButton"), CallBackData = ToJson($"Subscribe_{JsonSerializer.Serialize<SubscribeDetails>(new() { Type = "InnerSpreads", RequiredPage = requiredPage, InnerSpreadsId = id })}") }]);

                        await SendMessage(userSettings.TelegramId, text, [.. inlineKeyboardMarkups], true);
                    }
                }
            }

            foreach (UserSettings userSettings in usersSettings)
            {
                tasks.Add(Send(userSettings));
            }

            await Task.WhenAll(tasks);
        }

        public async Task UpdateSubscribedUsers(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(50, cancellationToken);

                    UserSettings[] usersSettings = await userService.GetUsersSettings();
                    List<Task> tasks = [];

                    async Task Update(UserSettings userSettings)
                    {
                        if (userSettings.SubscribedItem is not null)
                        {
                            SubscribeDetails subscribeItem = JsonSerializer.Deserialize<SubscribeDetails>(userSettings.SubscribedItem)!;

                            if (TimeSpan.FromSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - userSettings.LastActionDate).TotalMinutes < 10)
                            {
                                if (subscribeItem.Type == "Spreads" && subscribeItem.RequiredPage.HasValue)
                                {
                                    OrderedDictionary<string, OrderedDictionary<string, Spread>> innerSpreads = CexCompareService.GetInnerSpreads(
                                        userSettings.Profit, userSettings.Amount,
                                        JsonSerializer.Deserialize<Dictionary<string, SpreadsBlacklistItem>>(userSettings.SpreadsBlacklist) ?? [],
                                        JsonSerializer.Deserialize<Dictionary<string, CoinsBlacklistItem>>(userSettings.CoinsBlacklist) ?? [],
                                        JsonSerializer.Deserialize<HashSet<int>>(userSettings.BuyExBlacklist) ?? [],
                                        JsonSerializer.Deserialize<HashSet<int>>(userSettings.SellExBlacklist) ?? []);

                                    if (FillSpreads(innerSpreads, userSettings, subscribeItem) is (string text, List<TelegramInlineKeyboardMarkup[]> inlineKeyboardMarkups, int requiredPage))
                                    {
                                        if (requiredPage != subscribeItem.RequiredPage.Value)
                                        {
                                            subscribeItem.RequiredPage = requiredPage;
                                            await userService.UpdateUserSubscribedItem(userSettings.TelegramId, JsonSerializer.Serialize(subscribeItem));
                                        }

                                        inlineKeyboardMarkups.Add([new() { Text = languages.GetString(userSettings.LanguageCode, "UnsubscribeButton"), CallBackData = ToJson($"Unsubscribe_{JsonSerializer.Serialize(subscribeItem)}") }]);

                                        if (!String.IsNullOrWhiteSpace(subscribeItem.BackString))
                                        {
                                            inlineKeyboardMarkups.Add([new() { Text = languages.GetString(userSettings.LanguageCode, "BackButton"), CallBackData = ToJson(subscribeItem.BackString) }]);
                                        }

                                        await userService.UpdateUserSubscribedItemLastMessage(userSettings.TelegramId, JsonSerializer.Serialize(new SubscribeLastMessage() { Text = text, InlineKeyboardMarkups = [.. inlineKeyboardMarkups] }));

                                        if (subscribeItem.MessageId != -1)
                                        {
                                            await EditMessage(userSettings.TelegramId, subscribeItem.MessageId, text, [.. inlineKeyboardMarkups], true);
                                        }

                                        else
                                        {
                                            int? messageId = await SendMessage(userSettings.TelegramId, text, [.. inlineKeyboardMarkups], true);

                                            if (messageId.HasValue)
                                            {
                                                subscribeItem.MessageId = messageId.Value;
                                                await userService.UpdateUserSubscribedItem(userSettings.TelegramId, JsonSerializer.Serialize(subscribeItem));
                                            }
                                        }

                                        await Task.Delay(3000, cancellationToken);
                                    }
                                }

                                else if (subscribeItem.Type == "InnerSpreads" && subscribeItem.InnerSpreadsId is not null && subscribeItem.RequiredPage.HasValue)
                                {
                                    OrderedDictionary<string, Spread> spreads = CexCompareService.GetSpreads(
                                    subscribeItem.InnerSpreadsId, userSettings.Profit, userSettings.Amount,
                                    JsonSerializer.Deserialize<Dictionary<string, SpreadsBlacklistItem>>(userSettings.SpreadsBlacklist) ?? [],
                                    JsonSerializer.Deserialize<Dictionary<string, CoinsBlacklistItem>>(userSettings.CoinsBlacklist) ?? [],
                                    JsonSerializer.Deserialize<HashSet<int>>(userSettings.BuyExBlacklist) ?? [],
                                    JsonSerializer.Deserialize<HashSet<int>>(userSettings.SellExBlacklist) ?? []);

                                    if (FillInnerSpreads(spreads, userSettings, subscribeItem) is (string text, List<TelegramInlineKeyboardMarkup[]> inlineKeyboardMarkups, int requiredPage))
                                    {
                                        inlineKeyboardMarkups.Add([new() { Text = languages.GetString(userSettings.LanguageCode, "UnsubscribeButton"), CallBackData = ToJson($"Unsubscribe_{JsonSerializer.Serialize(subscribeItem)}") }]);

                                        if (!String.IsNullOrWhiteSpace(subscribeItem.BackString))
                                        {
                                            inlineKeyboardMarkups.Add([new() { Text = languages.GetString(userSettings.LanguageCode, "BackButton"), CallBackData = ToJson(subscribeItem.BackString) }]);
                                        }

                                        await userService.UpdateUserSubscribedItemLastMessage(userSettings.TelegramId, JsonSerializer.Serialize(new SubscribeLastMessage() { Text = text, InlineKeyboardMarkups = [.. inlineKeyboardMarkups] }));

                                        if (subscribeItem.MessageId != -1)
                                        {
                                            await EditMessage(userSettings.TelegramId, subscribeItem.MessageId, text, [.. inlineKeyboardMarkups], true);
                                        }

                                        else
                                        {
                                            int? messageId = await SendMessage(userSettings.TelegramId, text, [.. inlineKeyboardMarkups], true);

                                            if (messageId.HasValue)
                                            {
                                                subscribeItem.MessageId = messageId.Value;
                                                await userService.UpdateUserSubscribedItem(userSettings.TelegramId, JsonSerializer.Serialize(subscribeItem));
                                            }
                                        }

                                        await Task.Delay(3000, cancellationToken);
                                    }
                                }

                                else if (subscribeItem.Type == "Spread" && subscribeItem.SpreadId is not null)
                                {
                                    Spread? spread = CexCompareService.GetSpread(
                                        subscribeItem.SpreadId, double.NegativeInfinity, userSettings.Amount,
                                        JsonSerializer.Deserialize<Dictionary<string, SpreadsBlacklistItem>>(userSettings.SpreadsBlacklist) ?? [],
                                        JsonSerializer.Deserialize<Dictionary<string, CoinsBlacklistItem>>(userSettings.CoinsBlacklist) ?? [],
                                        JsonSerializer.Deserialize<HashSet<int>>(userSettings.BuyExBlacklist) ?? [],
                                        JsonSerializer.Deserialize<HashSet<int>>(userSettings.SellExBlacklist) ?? []);

                                    if (spread is not null)
                                    {
                                        string text = FillSpread(spread, userSettings.LanguageCode);

                                        List<TelegramInlineKeyboardMarkup[]> telegramInlineKeyboardMarkups = [
                                            [new() { Text = languages.GetString(userSettings.LanguageCode, "UnsubscribeButton"), CallBackData = ToJson($"Unsubscribe_{JsonSerializer.Serialize(subscribeItem)}") }]];

                                        if (!String.IsNullOrWhiteSpace(subscribeItem.BackString))
                                        {
                                            telegramInlineKeyboardMarkups.Add([new() { Text = languages.GetString(userSettings.LanguageCode, "BackButton"), CallBackData = ToJson(subscribeItem.BackString) }]);
                                        }

                                        await userService.UpdateUserSubscribedItemLastMessage(userSettings.TelegramId, JsonSerializer.Serialize(new SubscribeLastMessage() { Text = text, InlineKeyboardMarkups = [.. telegramInlineKeyboardMarkups] }));

                                        if (subscribeItem.MessageId != -1)
                                        {
                                            await EditMessage(userSettings.TelegramId, subscribeItem.MessageId, text, [.. telegramInlineKeyboardMarkups], true);
                                        }

                                        else
                                        {
                                            int? messageId = await SendMessage(userSettings.TelegramId, text, [.. telegramInlineKeyboardMarkups], true);

                                            if (messageId.HasValue)
                                            {
                                                subscribeItem.MessageId = messageId.Value;
                                                await userService.UpdateUserSubscribedItem(userSettings.TelegramId, JsonSerializer.Serialize(subscribeItem));
                                            }
                                        }

                                        await Task.Delay(3000, cancellationToken);
                                    }

                                    else
                                    {
                                        await Unsubscribe(userSettings.TelegramId, "*");
                                    }
                                }
                            }

                            else
                            {
                                await Unsubscribe(userSettings.TelegramId, "*");
                            }
                        }
                    }

                    foreach (UserSettings user in usersSettings)
                    {
                        tasks.Add(Update(user));
                    }

                    await Task.WhenAll(tasks);
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "UpdateSubscribedUsers disabled!");
                throw;
            }
        }

        public async Task StartCommand(long telegramId, string languageCode = "en", int? replyMessageId = null)
        {
            if (await userService.GetUserSetUp(telegramId))
            {
                await Home(telegramId);
            }

            else
            {
                if (replyMessageId.HasValue)
                {
                    await SetMessageReaction(telegramId, (int)replyMessageId, "😈");
                }

                await LanguageSelect(telegramId, languageCode: languageCode);
            }
        }

        public async Task SettingsCommand(long telegramId, string languageCode, int? replyMessageId = null)
        {
            if (await userService.GetUserSetUp(telegramId))
            {
                await Settings(telegramId);
            }

            else
            {
                await StartCommand(telegramId, languageCode, replyMessageId);
            }
        }

        public async Task SpreadsCommand(long telegramId, string languageCode, int? replyMessageId = null)
        {
            if (await userService.GetUserSetUp(telegramId))
            {
                await Spreads(telegramId, JsonSerializer.Serialize<SubscribeDetails>(new() { Type = "Spreads", RequiredPage = 1 }));
            }

            else
            {
                await StartCommand(telegramId, languageCode, replyMessageId);
            }
        }

        public async Task Home(long telegramId, int? messageId = null)
        {
            if (!await userService.GetUserSetUp(telegramId))
            {
                await StartCommand(telegramId);
                return;
            }

            await Unsubscribe(telegramId, "*");

            TelegramInlineKeyboardMarkup[][] telegramInlineKeyboardMarkups = [
                [new() { Text = languages.GetString(await userService.GetUserLanguageCode(telegramId), "SpreadsButton"), CallBackData = ToJson($"Spreads_{JsonSerializer.Serialize<SubscribeDetails>(new() { Type = "Spreads", RequiredPage = 1, BackString = "Home" })}") }],
                [new() { Text = languages.GetString(await userService.GetUserLanguageCode(telegramId), "SettingsButton"), CallBackData = ToJson("Settings_Home") }],
                [new() { Text = languages.GetString(await userService.GetUserLanguageCode(telegramId), "SupportButton"), Url = "t.me/Givemetooo" }]];

            if (messageId.HasValue)
            {
                await EditMessage(telegramId, (int)messageId, "Home", telegramInlineKeyboardMarkups);
            }

            else
            {
                await SendMessage(telegramId, "Home", telegramInlineKeyboardMarkups);
            }
        }

        public async Task Settings(long telegramId, int? messageId = null, string? backString = null)
        {
            if (await userService.GetUserSetUp(telegramId))
            {
                UserSettings userSettings = await userService.GetUserSettings(telegramId);

                string text = languages.GetString(await userService.GetUserLanguageCode(telegramId), "Settings");
                text = text.Replace("{minimum profit}", EscapeMarkdownV2(userSettings.Profit.ToString()));
                text = text.Replace("{maximum amount}", EscapeMarkdownV2(userSettings.Amount.ToString()));
                text = text.Replace("{buy exchanges}", Math.Max(cexsLoad.CexsCount() - JsonSerializer.Deserialize<HashSet<int>>(userSettings.BuyExBlacklist)!.Count, 0).ToString());
                text = text.Replace("{sell exchanges}", Math.Max(cexsLoad.CexsCount() - JsonSerializer.Deserialize<HashSet<int>>(userSettings.SellExBlacklist)!.Count, 0).ToString());
                text = text.Replace("{delay}", TimeToText(userSettings.LanguageCode, userSettings.NewSpreadsDelay));
                text = text.Replace("{blacklisted spreads}", JsonSerializer.Deserialize<Dictionary<string, SpreadsBlacklistItem>>(userSettings.SpreadsBlacklist)!.Count.ToString());
                text = text.Replace("{blacklisted coins}", JsonSerializer.Deserialize<Dictionary<string, CoinsBlacklistItem>>(userSettings.CoinsBlacklist)!.Count.ToString());

                text = text.Replace("{language}", languages.GetString(await userService.GetUserLanguageCode(telegramId), "LanguageName"));

                List<TelegramInlineKeyboardMarkup[]> telegramInlineKeyboardMarkups = [
                    [new() { Text = languages.GetString(await userService.GetUserLanguageCode(telegramId), "EditProfitButton"), CallBackData = ToJson($"SetProfit_Settings_{backString}") }, new() { Text = languages.GetString(await userService.GetUserLanguageCode(telegramId), "EditAmountButton"), CallBackData = ToJson($"SetAmount_Settings_{backString}") }],
                    [new() { Text = languages.GetString(await userService.GetUserLanguageCode(telegramId), "ChangeLanguageButton"), CallBackData = ToJson($"LanguageSelect_Settings_{backString}") }],
                    [new() { Text = languages.GetString(await userService.GetUserLanguageCode(telegramId), "BuyExchangesButton"), CallBackData = ToJson($"BuyExchanges_Settings_{backString}") }, new() { Text = languages.GetString(await userService.GetUserLanguageCode(telegramId), "SellExchangesButton"), CallBackData = ToJson($"SellExchanges_Settings_{backString}") }],
                    [new() { Text = languages.GetString(await userService.GetUserLanguageCode(telegramId), "EditDelayButton"), CallBackData = ToJson($"SetDelay_Settings_{backString}") }],
                    [new() { Text = languages.GetString(await userService.GetUserLanguageCode(telegramId), "SpreadsBlacklistButton"), CallBackData = ToJson("TEST") }, new() { Text = languages.GetString(await userService.GetUserLanguageCode(telegramId), "CoinsBlacklistButton"), CallBackData = ToJson("TEST") }]];

                if (!String.IsNullOrWhiteSpace(backString))
                {
                    telegramInlineKeyboardMarkups.Add([new() { Text = languages.GetString(await userService.GetUserLanguageCode(telegramId), "BackButton"), CallBackData = ToJson(backString) }]);
                }

                if (messageId.HasValue)
                {
                    await EditMessage(telegramId, (int)messageId, text, [.. telegramInlineKeyboardMarkups], true);
                }

                else
                {
                    await SendMessage(telegramId, text, [.. telegramInlineKeyboardMarkups], true);
                }
            }

            else
            {
                await StartCommand(telegramId);
            }
        }

        public async Task Spread(long telegramId, string data, int messageId = -1, string? callBackQueryId = null)
        {
            if (!await userService.GetUserSetUp(telegramId))
            {
                await StartCommand(telegramId);
                return;
            }

            await Unsubscribe(telegramId, "*");

            SubscribeDetails? subscribeItem = JsonSerializer.Deserialize<SubscribeDetails>(data);

            if (subscribeItem is not null && subscribeItem.Type == "Spread" && !String.IsNullOrWhiteSpace(subscribeItem.SpreadId))
            {
                UserSettings userSettings = await userService.GetUserSettings(telegramId);

                Spread? spread = CexCompareService.GetSpread(subscribeItem.SpreadId, double.NegativeInfinity, userSettings.Amount,
                    JsonSerializer.Deserialize<Dictionary<string, SpreadsBlacklistItem>>(userSettings.SpreadsBlacklist) ?? [],
                    JsonSerializer.Deserialize<Dictionary<string, CoinsBlacklistItem>>(userSettings.CoinsBlacklist) ?? [],
                    JsonSerializer.Deserialize<HashSet<int>>(userSettings.BuyExBlacklist) ?? [],
                    JsonSerializer.Deserialize<HashSet<int>>(userSettings.SellExBlacklist) ?? []);

                if (spread is not null)
                {
                    string text = FillSpread(spread, await userService.GetUserLanguageCode(telegramId));

                    List<TelegramInlineKeyboardMarkup[]> telegramInlineKeyboardMarkups = [
                        [new() { Text = languages.GetString(await userService.GetUserLanguageCode(telegramId), "SubscribeButton"), CallBackData = ToJson($"Subscribe_{JsonSerializer.Serialize(subscribeItem)}") }]];

                    //await AnswerCallbackQuery(callBackQueryId, languagesService.GetString(await userService.GetUserLanguageCode(telegramId), "SpreadSentNotification"));

                    if (!String.IsNullOrWhiteSpace(subscribeItem.BackString))
                    {
                        telegramInlineKeyboardMarkups.Add([new() { Text = languages.GetString(userSettings.LanguageCode, "BackButton"), CallBackData = ToJson(subscribeItem.BackString) }]);
                    }

                    if (messageId != -1)
                    {
                        await EditMessage(telegramId, messageId, text, [.. telegramInlineKeyboardMarkups], true);
                    }

                    else
                    {
                        await SendMessage(telegramId, text, [.. telegramInlineKeyboardMarkups], true);
                    }
                }

                else if (!String.IsNullOrWhiteSpace(callBackQueryId))
                {
                    await AnswerCallbackQuery(callBackQueryId, languages.GetString(await userService.GetUserLanguageCode(telegramId), "SubscribeFail"));
                }
            }
        }

        public async Task InnerSpreads(long telegramId, string data, int messageId = -1, string? callBackQueryId = null)
        {
            if (callBackQueryId is not null)
            {
                _ = AnswerCallbackQuery(callBackQueryId);
            }

            if (!await userService.GetUserSetUp(telegramId))
            {
                await StartCommand(telegramId);
                return;
            }

            await Unsubscribe(telegramId, "*");

            SubscribeDetails? subscribeItem = JsonSerializer.Deserialize<SubscribeDetails>(data);

            if (subscribeItem is not null && subscribeItem.Type == "InnerSpreads" && subscribeItem.InnerSpreadsId is not null && subscribeItem.RequiredPage.HasValue)
            {
                UserSettings userSettings = await userService.GetUserSettings(telegramId);

                OrderedDictionary<string, Spread> spreads = CexCompareService.GetSpreads(
                    subscribeItem.InnerSpreadsId, userSettings.Profit, userSettings.Amount,
                    JsonSerializer.Deserialize<Dictionary<string, SpreadsBlacklistItem>>(userSettings.SpreadsBlacklist) ?? [],
                    JsonSerializer.Deserialize<Dictionary<string, CoinsBlacklistItem>>(userSettings.CoinsBlacklist) ?? [],
                    JsonSerializer.Deserialize<HashSet<int>>(userSettings.BuyExBlacklist) ?? [],
                    JsonSerializer.Deserialize<HashSet<int>>(userSettings.SellExBlacklist) ?? []);

                if (FillInnerSpreads(spreads, userSettings, subscribeItem) is (string text, List<TelegramInlineKeyboardMarkup[]> inlineKeyboardMarkups, _))
                {
                    inlineKeyboardMarkups.Add([new() { Text = languages.GetString(userSettings.LanguageCode, "SubscribeButton"), CallBackData = ToJson($"Subscribe_{JsonSerializer.Serialize(subscribeItem)}") }]);

                    if (!String.IsNullOrWhiteSpace(subscribeItem.BackString))
                    {
                        inlineKeyboardMarkups.Add([new() { Text = languages.GetString(userSettings.LanguageCode, "BackButton"), CallBackData = ToJson(subscribeItem.BackString) }]);
                    }

                    if (messageId != -1)
                    {
                        await EditMessage(telegramId, messageId, text, [.. inlineKeyboardMarkups], true);
                    }

                    else
                    {
                        await SendMessage(telegramId, text, [.. inlineKeyboardMarkups], true);
                    }
                }
            }
        }

        public async Task Spreads(long telegramId, string data, int messageId = -1, string? callBackQueryId = null)
        {
            if (callBackQueryId is not null)
            {
                _ = AnswerCallbackQuery(callBackQueryId);
            }

            if (!await userService.GetUserSetUp(telegramId))
            {
                await StartCommand(telegramId);
                return;
            }

            await Unsubscribe(telegramId, "*");

            SubscribeDetails? subscribeItem = JsonSerializer.Deserialize<SubscribeDetails>(data);

            if (subscribeItem is not null && subscribeItem.Type == "Spreads" && subscribeItem.RequiredPage.HasValue)
            {
                UserSettings userSettings = await userService.GetUserSettings(telegramId);

                OrderedDictionary<string, OrderedDictionary<string, Spread>> innerSpreads = CexCompareService.GetInnerSpreads(
                    userSettings.Profit, userSettings.Amount,
                    JsonSerializer.Deserialize<Dictionary<string, SpreadsBlacklistItem>>(userSettings.SpreadsBlacklist) ?? [],
                    JsonSerializer.Deserialize<Dictionary<string, CoinsBlacklistItem>>(userSettings.CoinsBlacklist) ?? [],
                    JsonSerializer.Deserialize<HashSet<int>>(userSettings.BuyExBlacklist) ?? [],
                    JsonSerializer.Deserialize<HashSet<int>>(userSettings.SellExBlacklist) ?? []);

                if (FillSpreads(innerSpreads, userSettings, subscribeItem) is (string text, List<TelegramInlineKeyboardMarkup[]> inlineKeyboardMarkups, _))
                {
                    inlineKeyboardMarkups.Add([new() { Text = languages.GetString(userSettings.LanguageCode, "SubscribeButton"), CallBackData = ToJson($"Subscribe_{JsonSerializer.Serialize(subscribeItem)}") }]);

                    if (!String.IsNullOrWhiteSpace(subscribeItem.BackString))
                    {
                        inlineKeyboardMarkups.Add([new() { Text = languages.GetString(userSettings.LanguageCode, "BackButton"), CallBackData = ToJson(subscribeItem.BackString) }]);
                    }

                    if (messageId != -1)
                    {
                        await EditMessage(telegramId, messageId, text, [.. inlineKeyboardMarkups], true);
                    }

                    else
                    {
                        await SendMessage(telegramId, text, [.. inlineKeyboardMarkups], true);
                    }
                }
            }
        }

        public async Task Disagree(long telegramId, string callBackQueryId)
        {
            _ = AnswerCallbackQuery(callBackQueryId);

            if (!await userService.DoesUserExist(telegramId) || await userService.GetUserSetUp(telegramId))
            {
                await StartCommand(telegramId);
                return;
            }

            string languageCode = await userService.GetUserLanguageCode(telegramId);

            if (await userService.DeleteUser(telegramId))
            {
                await AnswerCallbackQuery(callBackQueryId, languages.GetString(languageCode, "DisagreeNotification"));
            }
        }

        public async Task Conditions(long telegramId, int? messageId = null)
        {
            if (!await userService.DoesUserExist(telegramId) || await userService.GetUserSetUp(telegramId))
            {
                await StartCommand(telegramId);
                return;
            }

            TelegramInlineKeyboardMarkup[][] telegramInlineKeyboardMarkups = [
                [new() { Text = languages.GetString(await userService.GetUserLanguageCode(telegramId), "AgreeButton"), CallBackData = ToJson("UpdateUserSetUp","Home") }],
                [new() { Text = languages.GetString(await userService.GetUserLanguageCode(telegramId), "DisagreeButton"), CallBackData = ToJson("Disagree") }]];

            if (messageId.HasValue)
            {
                await EditMessage(telegramId, messageId.Value, "Conditions", telegramInlineKeyboardMarkups);
            }

            else
            {
                await SendMessage(telegramId, "Conditions", telegramInlineKeyboardMarkups);
            }
        }

        public async Task LanguageSelect(long telegramId, int? messageId = null, string languageCode = "en", string? backString = null)
        {
            string[] codes = languages.GetLanguageCodes();

            if (await userService.DoesUserExist(telegramId))
            {
                languageCode = await userService.GetUserLanguageCode(telegramId);
            }

            if (await userService.GetUserSetUp(telegramId))
            {
                List<TelegramInlineKeyboardMarkup> buttons = [];

                for (int i = 0; i < codes.Length; i++)
                {
                    if (codes[i] != languageCode)
                    {
                        buttons.Add(new() { Text = languages.GetString(codes[i], "LanguageName"), CallBackData = ToJson($"UpdateUserLanguageCode_{codes[i]}", backString) });
                    }
                }

                List<TelegramInlineKeyboardMarkup[]> telegramInlineKeyboardMarkups = [[new() { Text = $"⭐️ {languages.GetString(await userService.GetUserLanguageCode(telegramId), "LanguageName")}", CallBackData = ToJson($"UpdateUserLanguageCode_{languageCode}", backString) }], [.. buttons]];

                if (!String.IsNullOrWhiteSpace(backString))
                {
                    telegramInlineKeyboardMarkups.Add([new() { Text = languages.GetString(await userService.GetUserLanguageCode(telegramId), "BackButton"), CallBackData = ToJson(backString) }]);
                }

                if (messageId.HasValue)
                {
                    await EditMessage(telegramId, messageId.Value, "LanguageSelect", [.. telegramInlineKeyboardMarkups]);
                }

                else
                {
                    await SendMessage(telegramId, "LanguageSelect", [.. telegramInlineKeyboardMarkups]);
                }
            }

            else
            {
                List<TelegramInlineKeyboardMarkup> buttons = [];

                for (int i = 0; i < codes.Length; i++)
                {
                    if (codes[i] != languageCode)
                    {
                        buttons.Add(new() { Text = languages.GetString(codes[i], "LanguageName"), CallBackData = ToJson($"UpdateUserLanguageCode_{codes[i]}", "WelcomeInfo_LanguageSelect_") });
                    }
                }

                TelegramInlineKeyboardMarkup[][] telegramInlineKeyboardMarkups = codes.Contains(languageCode) ? [[new() { Text = $"⭐️ {languages.GetString(languageCode, "LanguageName")}", CallBackData = ToJson($"UpdateUserLanguageCode_{languageCode}", "WelcomeInfo_LanguageSelect_") }], [.. buttons]] : [[.. buttons]];

                if (messageId.HasValue)
                {
                    await EditMessage(telegramId, messageId.Value, "LanguageSelectWelcome", [.. telegramInlineKeyboardMarkups]);
                }

                else
                {
                    await SendMessage(telegramId, "LanguageSelectWelcome", [.. telegramInlineKeyboardMarkups], languageCode: languageCode);
                }
            }
        }

        public async Task WelcomeInfo(long telegramId, int? messageId = null, string? backString = null)
        {
            if (!await userService.DoesUserExist(telegramId) || await userService.GetUserSetUp(telegramId))
            {
                await StartCommand(telegramId);
                return;
            }

            UserSettings userSettings = await userService.GetUserSettings(telegramId);

            List<TelegramInlineKeyboardMarkup[]> telegramInlineKeyboardMarkups = [
                [new() { Text = languages.GetString(userSettings.LanguageCode, "EditProfitButton"), CallBackData = ToJson($"SetProfit_WelcomeInfo_{backString}") }],
                [new() { Text = languages.GetString(userSettings.LanguageCode, "EditAmountButton"), CallBackData = ToJson($"SetAmount_WelcomeInfo_{backString}") }],
                [new() { Text = languages.GetString(userSettings.LanguageCode, "SkipButton"), CallBackData = ToJson("Conditions") }]];

            if (!String.IsNullOrWhiteSpace(backString))
            {
                telegramInlineKeyboardMarkups.Add([new() { Text = languages.GetString(userSettings.LanguageCode, "BackButton"), CallBackData = ToJson(backString) }]);
            }

            string text = languages.GetString(userSettings.LanguageCode, "WelcomeInfo");
            text = text.Replace("{minimum profit}", EscapeMarkdownV2(userSettings.Profit.ToString()));
            text = text.Replace("{maximum amount}", EscapeMarkdownV2(userSettings.Amount.ToString()));

            if (messageId.HasValue)
            {
                await EditMessage(telegramId, (int)messageId, text, [.. telegramInlineKeyboardMarkups], true);
            }

            else
            {
                await SendMessage(telegramId, text, [.. telegramInlineKeyboardMarkups], true);
            }
        }

        public async Task SetProfit(long telegramId, int? messageId = null, string? backString = null)
        {
            if (!await userService.UpdateUserWaitingMethods(telegramId, JsonSerializer.Serialize<string[]>([$"UpdateUserProfit_{backString}"]))) //Checking is user registered and updating waiting method
            {
                await StartCommand(telegramId);
                return;
            }

            TelegramInlineKeyboardMarkup[][]? telegramInlineKeyboardMarkups = null;

            if (!String.IsNullOrWhiteSpace(backString))
            {
                telegramInlineKeyboardMarkups = [[new() { Text = languages.GetString(await userService.GetUserLanguageCode(telegramId), "BackButton"), CallBackData = ToJson(backString) }]];
            }

            if (messageId.HasValue)
            {
                await EditMessage(telegramId, messageId.Value, "SetProfit", telegramInlineKeyboardMarkups);
            }

            else
            {
                await SendMessage(telegramId, "SetProfit", telegramInlineKeyboardMarkups);
            }
        }

        public async Task SetAmount(long telegramId, int? messageId = null, string? backString = null)
        {
            if (!await userService.UpdateUserWaitingMethods(telegramId, JsonSerializer.Serialize<string[]>([$"UpdateUserAmount_{backString}"]))) //Checking is user registered and updating waiting method
            {
                await StartCommand(telegramId);
                return;
            }

            TelegramInlineKeyboardMarkup[][]? telegramInlineKeyboardMarkups = null;

            if (!String.IsNullOrWhiteSpace(backString))
            {
                telegramInlineKeyboardMarkups = [[new() { Text = languages.GetString(await userService.GetUserLanguageCode(telegramId), "BackButton"), CallBackData = ToJson(backString) }]];
            }

            if (messageId.HasValue)
            {
                await EditMessage(telegramId, messageId.Value, "SetAmount", telegramInlineKeyboardMarkups);
            }

            else
            {
                await SendMessage(telegramId, "SetAmount", telegramInlineKeyboardMarkups);
            }
        }

        public async Task SetDelay(long telegramId, int? messageId = null, string? backString = null)
        {
            if (!await userService.UpdateUserWaitingMethods(telegramId, JsonSerializer.Serialize<string[]>([$"UpdateUserDelay_{backString}"]))) //Checking is user registered and updating waiting method
            {
                await StartCommand(telegramId);
                return;
            }

            TelegramInlineKeyboardMarkup[][]? telegramInlineKeyboardMarkups = null;

            if (!String.IsNullOrWhiteSpace(backString))
            {
                telegramInlineKeyboardMarkups = [[new() { Text = languages.GetString(await userService.GetUserLanguageCode(telegramId), "BackButton"), CallBackData = ToJson(backString) }]];
            }

            if (messageId.HasValue)
            {
                await EditMessage(telegramId, messageId.Value, "SetDelay", telegramInlineKeyboardMarkups);
            }

            else
            {
                await SendMessage(telegramId, "SetDelay", telegramInlineKeyboardMarkups);
            }
        }

        public async Task UpdateUserProfit(long telegramId, int replyMessageId, string data, string? backString = null)
        {
            if (!String.IsNullOrWhiteSpace(data) && double.TryParse(data.ToLower().Replace(",", ".").Replace("$", "").Replace(" ", "").Replace("usd", ""), out double profit))
            {
                if (profit >= 1)
                {
                    await userService.UpdateUserProfit(telegramId, Math.Round(profit, 2));

                    if (!String.IsNullOrWhiteSpace(backString))
                    {
                        await Methods(backString, telegramId);
                    }
                }

                else
                {
                    await SetMessageReaction(telegramId, replyMessageId, "🤷‍♀");
                    await SendMessage(telegramId, "SetProfitValueWarn", null, false, null, replyMessageId);
                    await userService.UpdateUserWaitingMethods(telegramId, JsonSerializer.Serialize<string[]>([$"UpdateUserProfit_{backString}"]));
                }
            }

            else
            {
                await SetMessageReaction(telegramId, replyMessageId, "🤷‍♀");
                await SendMessage(telegramId, "SetProfitFormatWarn", null, false, null, replyMessageId);
                await userService.UpdateUserWaitingMethods(telegramId, JsonSerializer.Serialize<string[]>([$"UpdateUserProfit_{backString}"]));
            }
        }

        public async Task UpdateUserAmount(long telegramId, int replyMessageId, string data, string? backString = null)
        {
            if (!String.IsNullOrWhiteSpace(data) && double.TryParse(data.ToLower().Replace(",", ".").Replace("$", "").Replace(" ", "").Replace("usd", ""), out double amount))
            {
                if (amount >= 5)
                {
                    await userService.UpdateUserAmount(telegramId, Math.Round(amount, 2));

                    if (!String.IsNullOrWhiteSpace(backString))
                    {
                        await Methods(backString, telegramId);
                    }
                }

                else
                {
                    await SetMessageReaction(telegramId, replyMessageId, "🤷‍♀");
                    await SendMessage(telegramId, "SetAmountValueWarn", null, false, null, replyMessageId);
                    await userService.UpdateUserWaitingMethods(telegramId, JsonSerializer.Serialize<string[]>([$"UpdateUserAmount_{backString}"]));
                }
            }

            else
            {
                await SetMessageReaction(telegramId, replyMessageId, "🤷‍♀");
                await SendMessage(telegramId, "SetAmountFormatWarn", null, false, null, replyMessageId);
                await userService.UpdateUserWaitingMethods(telegramId, JsonSerializer.Serialize<string[]>([$"UpdateUserAmount_{backString}"]));
            }
        }

        public async Task UpdateUserDelay(long telegramId, int replyMessageId, string data, string? backString = null)
        {
            if (!String.IsNullOrWhiteSpace(data))
            {
                data = data.ToLower().Replace(" ", "");

                bool isValid = false;
                string[] secondNames = languages.GetString(await userService.GetUserLanguageCode(telegramId), "SecondNames").Split(",");
                string[] minuteNames = languages.GetString(await userService.GetUserLanguageCode(telegramId), "MinuteNames").Split(",");

                foreach (string secondName in secondNames)
                {
                    if (data.EndsWith(secondName) && int.TryParse(data[..(data.Length - secondName.Length)], out int seconds) && seconds >= 0)
                    {
                        await userService.UpdateUserNewSpreadsDelay(telegramId, seconds);
                        isValid = true;

                        if (!String.IsNullOrWhiteSpace(backString))
                        {
                            await Methods(backString, telegramId);
                        }
                        break;
                    }
                }

                if (!isValid)
                {
                    foreach (string minuteName in minuteNames)
                    {
                        if (data.EndsWith(minuteName) && int.TryParse(data[..(data.Length - minuteName.Length)], out int minutes) && minutes >= 0)
                        {
                            await userService.UpdateUserNewSpreadsDelay(telegramId, minutes * 60);
                            isValid = true;

                            if (!String.IsNullOrWhiteSpace(backString))
                            {
                                await Methods(backString, telegramId);
                            }
                            break;
                        }
                    }
                }

                if (!isValid)
                {
                    await SetMessageReaction(telegramId, replyMessageId, "🤷‍♀");
                    await SendMessage(telegramId, "SetDelayFormatWarn", null, false, null, replyMessageId);
                    await userService.UpdateUserWaitingMethods(telegramId, JsonSerializer.Serialize<string[]>([$"UpdateUserDelay_{backString}"]));
                }
            }
        }

        public async Task UpdateUserSetUp(long telegramId, bool status)
        {
            await userService.UpdateUserSetUp(telegramId, status);
        }

        public async Task UpdateUserLanguageCode(long telegramId, string callBackQueryId, string languageCode)
        {
            languageCode = languages.GetLanguageCodes().Contains(languageCode) ? languageCode : "en";

            _ = AnswerCallbackQuery(callBackQueryId, languages.GetString(languageCode, "LanguageChanged"));

            if (!await userService.UpdateUserLanguageCode(telegramId, languageCode))
            {
                await userService.AddUser(telegramId, languageCode);
            }
        }

        public async Task Subscribe(long telegramId, int messageId, string callBackQueryId, string data)
        {
            if (!await userService.GetUserSetUp(telegramId))
            {
                await StartCommand(telegramId);
                return;
            }

            await Unsubscribe(telegramId, "*");
            SubscribeDetails? subscribedItem = JsonSerializer.Deserialize<SubscribeDetails>(data);

            if (subscribedItem is not null && subscribedItem.Type == "Spread" && subscribedItem.SpreadId is not null && CexCompareService.IsSpreadActual(subscribedItem.SpreadId))
            {
                subscribedItem.MessageId = messageId;

                if (await userService.UpdateUserSubscribedItem(telegramId, JsonSerializer.Serialize(subscribedItem)))
                {
                    await AnswerCallbackQuery(callBackQueryId, languages.GetString(await userService.GetUserLanguageCode(telegramId), "Subscribed"));
                }
            }

            else if (subscribedItem is not null && subscribedItem.Type != "Spread")
            {
                subscribedItem.MessageId = messageId;

                if (await userService.UpdateUserSubscribedItem(telegramId, JsonSerializer.Serialize(subscribedItem)))
                {
                    await AnswerCallbackQuery(callBackQueryId, languages.GetString(await userService.GetUserLanguageCode(telegramId), "Subscribed"));
                }
            }

            else
            {
                await AnswerCallbackQuery(callBackQueryId, languages.GetString(await userService.GetUserLanguageCode(telegramId), "SubscribeFail"));
            }
        }

        public async Task Unsubscribe(long telegramId, string data, int? messageId = null, string? callBackQueryId = null, string? messageText = null, TelegramInlineKeyboardMarkup[][]? messageInlineKeyboard = null, TelegramMessageEntity[]? telegramMessageEntities = null)
        {
            if (!await userService.GetUserSetUp(telegramId))
            {
                await StartCommand(telegramId);
                return;
            }

            string? subscribedItemJson = await userService.GetUserSubscribedItem(telegramId);
            string? subscribedItemLastMessageJson = await userService.GetUserSubscribedItemLastMessage(telegramId);

            SubscribeDetails? subscribedItem = subscribedItemJson is not null ? JsonSerializer.Deserialize<SubscribeDetails>(subscribedItemJson)! : null;
            SubscribeLastMessage? subscribedItemLastMessage = subscribedItemLastMessageJson is not null ? JsonSerializer.Deserialize<SubscribeLastMessage>(subscribedItemLastMessageJson)! : null;

            if (data == "*")
            {
                await userService.UpdateUserSubscribedItem(telegramId, null);
                await userService.UpdateUserSubscribedItemLastMessage(telegramId, null);

                if (subscribedItem is not null && subscribedItemLastMessage is not null)
                {
                    string language = await userService.GetUserLanguageCode(telegramId);

                    TelegramInlineKeyboardMarkup[][] telegramInlineKeyboardMarkups = [.. subscribedItemLastMessage.InlineKeyboardMarkups.Select(markups => {

                        if (markups.Length > 0 && markups[0].CallBackData is { } cbd && cbd.StartsWith("[\"Unsubscribe_"))
                        {
                            return
                            [
                                new TelegramInlineKeyboardMarkup
                                {
                                    Text = languages.GetString(language, "SubscribeButton"),
                                    CallBackData = ToJson($"Subscribe_{JsonSerializer.Serialize(subscribedItem)}")
                                }
                            ];
                        }

                        return markups;
                    })];

                    await EditMessage(telegramId, subscribedItem.MessageId, subscribedItemLastMessage.Text, telegramInlineKeyboardMarkups, true);
                }
            }

            else if (messageId.HasValue && !String.IsNullOrWhiteSpace(messageText) && messageInlineKeyboard is not null)
            {
                SubscribeDetails itemToUnsubscribe = JsonSerializer.Deserialize<SubscribeDetails>(data)!;

                if (subscribedItem is not null &&
                    subscribedItem.Type == itemToUnsubscribe.Type &&
                    subscribedItem.MessageId == messageId.Value &&
                    subscribedItem.SpreadId == itemToUnsubscribe.SpreadId &&
                    subscribedItem.RequiredPage == itemToUnsubscribe.RequiredPage &&
                    subscribedItem.InnerSpreadsId == itemToUnsubscribe.InnerSpreadsId)
                {
                    await userService.UpdateUserSubscribedItem(telegramId, null);
                    await userService.UpdateUserSubscribedItemLastMessage(telegramId, null);
                }

                string language = await userService.GetUserLanguageCode(telegramId);

                TelegramInlineKeyboardMarkup[][] telegramInlineKeyboardMarkups = [.. messageInlineKeyboard.Select(markups => {

                    if (markups.Length > 0 && markups[0].CallBackData is { } cbd && cbd.StartsWith("[\"Unsubscribe_"))
                    {
                        return
                        [
                            new TelegramInlineKeyboardMarkup
                            {
                                Text = languages.GetString(language, "SubscribeButton"),
                                CallBackData = $"Subscribe_{JsonSerializer.Serialize(itemToUnsubscribe)}"
                            }
                        ];
                    }

                    return markups;
                })];

                await EditMessage(telegramId, messageId.Value, messageText, telegramInlineKeyboardMarkups, true, null, telegramMessageEntities);

                if (callBackQueryId is not null)
                {
                    await AnswerCallbackQuery(callBackQueryId, languages.GetString(await userService.GetUserLanguageCode(telegramId), "Unsubscribed"));
                }
            }
        }

        public async Task BuyExchanges(long telegramId, int messageId, string callBackQueryId, string? backString = null)
        {
            _ = AnswerCallbackQuery(callBackQueryId);

            if (!await userService.GetUserSetUp(telegramId))
            {
                await StartCommand(telegramId);
                return;
            }

            HashSet<int> buyExBlacklist = JsonSerializer.Deserialize<HashSet<int>>(await userService.GetUserBuyExBlacklist(telegramId))!;
            List<IExchange> cexs = [.. cexsLoad.GetCexs().Select(kv => kv.Value)];
            cexs = [.. cexs.OrderBy(ex => ex.ExchangeId)];

            List<TelegramInlineKeyboardMarkup[]> telegramInlineKeyboardMarkups = [];

            for (int i = 0; i < cexs.Count; i += 2)
            {
                TelegramInlineKeyboardMarkup button1 = new()
                {
                    Text = buyExBlacklist.Contains(cexs[i].ExchangeId) ? $"▶️ {cexs[i].ExchangeName}" : $"⏹️ {cexs[i].ExchangeName}",
                    CallBackData = ToJson(buyExBlacklist.Contains(cexs[i].ExchangeId) ? $"RemoveFromBuyExBlacklist_{cexs[i].ExchangeId}" : $"AddToBuyExBlacklist_{cexs[i].ExchangeId}", $"BuyExchanges_{backString}")
                };

                TelegramInlineKeyboardMarkup? button2 = i + 1 < cexs.Count ? new()
                {
                    Text = buyExBlacklist.Contains(cexs[i + 1].ExchangeId) ? $"▶️ {cexs[i + 1].ExchangeName}" : $"⏹️ {cexs[i + 1].ExchangeName}",
                    CallBackData = ToJson(buyExBlacklist.Contains(cexs[i + 1].ExchangeId) ? $"RemoveFromBuyExBlacklist_{cexs[i + 1].ExchangeId}" : $"AddToBuyExBlacklist_{cexs[i + 1].ExchangeId}", $"BuyExchanges_{backString}")
                } : null;

                telegramInlineKeyboardMarkups.Add(button2 is not null ? [button1, button2] : [button1]);
            }

            if (!String.IsNullOrWhiteSpace(backString))
            {
                telegramInlineKeyboardMarkups.Add([new() { Text = languages.GetString(await userService.GetUserLanguageCode(telegramId), "BackButton"), CallBackData = ToJson(backString) }]);
            }

            string text = languages.GetString(await userService.GetUserLanguageCode(telegramId), "BuyExBlacklist");
            text = text.Replace("{buyExCount}", Math.Max(cexs.Count - buyExBlacklist.Count, 0).ToString());

            await EditMessage(telegramId, messageId, text, [.. telegramInlineKeyboardMarkups], true);
        }

        public async Task SellExchanges(long telegramId, int messageId, string callBackQueryId, string? backString = null)
        {
            _ = AnswerCallbackQuery(callBackQueryId);

            if (!await userService.GetUserSetUp(telegramId))
            {
                await StartCommand(telegramId);
                return;
            }

            HashSet<int> sellExBlacklist = JsonSerializer.Deserialize<HashSet<int>>(await userService.GetUserSellExBlacklist(telegramId))!;
            List<IExchange> cexs = [.. cexsLoad.GetCexs().Select(kv => kv.Value)];
            cexs = [.. cexs.OrderBy(ex => ex.ExchangeId)];

            List<TelegramInlineKeyboardMarkup[]> telegramInlineKeyboardMarkups = [];

            for (int i = 0; i < cexs.Count; i += 2)
            {
                TelegramInlineKeyboardMarkup button1 = new()
                {
                    Text = sellExBlacklist.Contains(cexs[i].ExchangeId) ? $"▶️ {cexs[i].ExchangeName}" : $"⏹️ {cexs[i].ExchangeName}",
                    CallBackData = ToJson(sellExBlacklist.Contains(cexs[i].ExchangeId) ? $"RemoveFromSellExBlacklist_{cexs[i].ExchangeId}" : $"AddToSellExBlacklist_{cexs[i].ExchangeId}", $"SellExchanges_{backString}")
                };

                TelegramInlineKeyboardMarkup? button2 = i + 1 < cexs.Count ? new()
                {
                    Text = sellExBlacklist.Contains(cexs[i + 1].ExchangeId) ? $"▶️ {cexs[i + 1].ExchangeName}" : $"⏹️ {cexs[i + 1].ExchangeName}",
                    CallBackData = ToJson(sellExBlacklist.Contains(cexs[i + 1].ExchangeId) ? $"RemoveFromSellExBlacklist_{cexs[i + 1].ExchangeId}" : $"AddToSellExBlacklist_{cexs[i + 1].ExchangeId}", $"SellExchanges_{backString}")
                } : null;

                telegramInlineKeyboardMarkups.Add(button2 is not null ? [button1, button2] : [button1]);
            }

            if (!String.IsNullOrWhiteSpace(backString))
            {
                telegramInlineKeyboardMarkups.Add([new() { Text = languages.GetString(await userService.GetUserLanguageCode(telegramId), "BackButton"), CallBackData = ToJson(backString) }]);
            }

            string text = languages.GetString(await userService.GetUserLanguageCode(telegramId), "SellExBlacklist");
            text = text.Replace("{sellExCount}", Math.Max(cexs.Count - sellExBlacklist.Count, 0).ToString());

            await EditMessage(telegramId, messageId, text, [.. telegramInlineKeyboardMarkups], true);
        }

        public async Task AddToBuyExBlacklist(long telegramId, int id)
        {
            if (!await userService.GetUserSetUp(telegramId))
            {
                await StartCommand(telegramId);
                return;
            }

            HashSet<int> buyExBlacklist = JsonSerializer.Deserialize<HashSet<int>>(await userService.GetUserBuyExBlacklist(telegramId))!;
            buyExBlacklist.Add(id);

            await userService.UpdateUserBuyExBlacklist(telegramId, JsonSerializer.Serialize(buyExBlacklist));
        }

        public async Task RemoveFromBuyExBlacklist(long telegramId, int id)
        {
            if (!await userService.GetUserSetUp(telegramId))
            {
                await StartCommand(telegramId);
                return;
            }

            HashSet<int> buyExBlacklist = JsonSerializer.Deserialize<HashSet<int>>(await userService.GetUserBuyExBlacklist(telegramId))!;
            buyExBlacklist.Remove(id);

            await userService.UpdateUserBuyExBlacklist(telegramId, JsonSerializer.Serialize(buyExBlacklist));
        }

        public async Task AddToSellExBlacklist(long telegramId, int id)
        {
            if (!await userService.GetUserSetUp(telegramId))
            {
                await StartCommand(telegramId);
                return;
            }

            HashSet<int> sellExBlacklist = JsonSerializer.Deserialize<HashSet<int>>(await userService.GetUserSellExBlacklist(telegramId))!;
            sellExBlacklist.Add(id);

            await userService.UpdateUserSellExBlacklist(telegramId, JsonSerializer.Serialize(sellExBlacklist));
        }

        public async Task RemoveFromSellExBlacklist(long telegramId, int id)
        {
            if (!await userService.GetUserSetUp(telegramId))
            {
                await StartCommand(telegramId);
                return;
            }

            HashSet<int> sellExBlacklist = JsonSerializer.Deserialize<HashSet<int>>(await userService.GetUserSellExBlacklist(telegramId))!;
            sellExBlacklist.Remove(id);

            await userService.UpdateUserSellExBlacklist(telegramId, JsonSerializer.Serialize(sellExBlacklist));
        }

        public async Task Methods(string methodName, long telegramId, int? messageId = null, string? callBackQueryId = null, string? data = null, int? replyMessageId = null, string? messageText = null, TelegramInlineKeyboardMarkup[][]? messageInlineKeyboard = null, TelegramMessageEntity[]? telegramMessageEntities = null)
        {
            switch (methodName)
            {
                case "Home":
                    await Home(telegramId, messageId);
                    break;

                case "UpdateUserSetUp":
                    await UpdateUserSetUp(telegramId, true);
                    break;

                case "Conditions":
                    await Conditions(telegramId, messageId);
                    break;

                case "Disagree" when !String.IsNullOrWhiteSpace(callBackQueryId):
                    await Disagree(telegramId, callBackQueryId);
                    break;

                case { } when methodName.StartsWith("UpdateUserProfit_") && replyMessageId.HasValue && !String.IsNullOrWhiteSpace(data):
                    await UpdateUserProfit(telegramId, replyMessageId.Value, data, methodName["UpdateUserProfit_".Length..].NullIfWhiteSpace());
                    break;

                case { } when methodName.StartsWith("UpdateUserAmount_") && replyMessageId.HasValue && !String.IsNullOrWhiteSpace(data):
                    await UpdateUserAmount(telegramId, replyMessageId.Value, data, methodName["UpdateUserAmount_".Length..].NullIfWhiteSpace());
                    break;

                case { } when methodName.StartsWith("UpdateUserDelay_") && replyMessageId.HasValue && !String.IsNullOrWhiteSpace(data):
                    await UpdateUserDelay(telegramId, replyMessageId.Value, data, methodName["UpdateUserDelay_".Length..].NullIfWhiteSpace());
                    break;

                case { } when methodName.StartsWith("WelcomeInfo_"):
                    await WelcomeInfo(telegramId, messageId, methodName["WelcomeInfo_".Length..].NullIfWhiteSpace());
                    break;

                case { } when methodName.StartsWith("SetProfit_"):
                    await SetProfit(telegramId, messageId, methodName["SetProfit_".Length..].NullIfWhiteSpace());
                    break;

                case { } when methodName.StartsWith("SetAmount_"):
                    await SetAmount(telegramId, messageId, methodName["SetAmount_".Length..].NullIfWhiteSpace());
                    break;

                case { } when methodName.StartsWith("SetDelay_"):
                    await SetDelay(telegramId, messageId, methodName["SetDelay_".Length..].NullIfWhiteSpace());
                    break;

                case { } when methodName.StartsWith("Settings_"):
                    await Settings(telegramId, messageId, methodName["Settings_".Length..].NullIfWhiteSpace());
                    break;

                case { } when methodName.StartsWith("LanguageSelect_"):
                    await LanguageSelect(telegramId, messageId, backString: methodName["LanguageSelect_".Length..].NullIfWhiteSpace());
                    break;

                case { } when methodName.StartsWith("BuyExchanges_") && messageId.HasValue && !String.IsNullOrWhiteSpace(callBackQueryId):
                    await BuyExchanges(telegramId, messageId.Value, callBackQueryId, methodName["BuyExchanges_".Length..].NullIfWhiteSpace());
                    break;

                case { } when methodName.StartsWith("SellExchanges_") && messageId.HasValue && !String.IsNullOrWhiteSpace(callBackQueryId):
                    await SellExchanges(telegramId, messageId.Value, callBackQueryId, methodName["SellExchanges_".Length..].NullIfWhiteSpace());
                    break;

                case { } when methodName.StartsWith("Spreads_") && !String.IsNullOrWhiteSpace(methodName["Spreads_".Length..]) && messageId.HasValue && !String.IsNullOrWhiteSpace(callBackQueryId):
                    await Spreads(telegramId, methodName["Spreads_".Length..], messageId.Value, callBackQueryId);
                    break;

                case { } when methodName.StartsWith("InnerSpreads_") && !String.IsNullOrWhiteSpace(methodName["InnerSpreads_".Length..]) && messageId.HasValue && !String.IsNullOrWhiteSpace(callBackQueryId):
                    await InnerSpreads(telegramId, methodName["InnerSpreads_".Length..], messageId.Value, callBackQueryId);
                    break;

                case { } when methodName.StartsWith("Spread_") && !String.IsNullOrWhiteSpace(methodName["Spread_".Length..]) && messageId.HasValue && !String.IsNullOrWhiteSpace(callBackQueryId):
                    await Spread(telegramId, methodName["Spread_".Length..], messageId.Value, callBackQueryId);
                    break;

                case { } when methodName.StartsWith("Subscribe_") && !String.IsNullOrWhiteSpace(methodName["Subscribe_".Length..]) && messageId.HasValue && !String.IsNullOrWhiteSpace(callBackQueryId):
                    await Subscribe(telegramId, messageId.Value, callBackQueryId, methodName["Subscribe_".Length..]);
                    break;

                case { } when methodName.StartsWith("Unsubscribe_") && !String.IsNullOrWhiteSpace(methodName["Unsubscribe_".Length..]):
                    await Unsubscribe(telegramId, methodName["Unsubscribe_".Length..], messageId, callBackQueryId, messageText, messageInlineKeyboard, telegramMessageEntities);
                    break;

                case { } when methodName.StartsWith("UpdateUserLanguageCode_") && !String.IsNullOrWhiteSpace(methodName["UpdateUserLanguageCode_".Length..]) && !String.IsNullOrWhiteSpace(callBackQueryId):
                    await UpdateUserLanguageCode(telegramId, callBackQueryId, methodName["UpdateUserLanguageCode_".Length..]);
                    break;

                case { } when methodName.StartsWith("AddToBuyExBlacklist_") && int.TryParse(methodName["AddToBuyExBlacklist_".Length..], out int id):
                    await AddToBuyExBlacklist(telegramId, id);
                    break;

                case { } when methodName.StartsWith("RemoveFromBuyExBlacklist_") && int.TryParse(methodName["RemoveFromBuyExBlacklist_".Length..], out int id):
                    await RemoveFromBuyExBlacklist(telegramId, id);
                    break;

                case { } when methodName.StartsWith("AddToSellExBlacklist_") && int.TryParse(methodName["AddToSellExBlacklist_".Length..], out int id):
                    await AddToSellExBlacklist(telegramId, id);
                    break;

                case { } when methodName.StartsWith("RemoveFromSellExBlacklist_") && int.TryParse(methodName["RemoveFromSellExBlacklist_".Length..], out int id):
                    await RemoveFromSellExBlacklist(telegramId, id);
                    break;

                default:
                    break;
            }
        }

        public async Task<int?> SendMessage(long telegramId, string text, TelegramInlineKeyboardMarkup[][]? telegramInlineKeyboardMarkups = null, bool isText = false, string? languageCode = null, int? replyMessageId = null)
        {
            languageCode ??= await userService.GetUserLanguageCode(telegramId);

            TelegramSendMessage telegramSendMessage;

            if (isText)
            {
                if (telegramInlineKeyboardMarkups is not null)
                {
                    Dictionary<string, string> callBackDatas = [];

                    foreach (TelegramInlineKeyboardMarkup[] telegramInlineKeyboardMarkup in telegramInlineKeyboardMarkups)
                    {
                        foreach (TelegramInlineKeyboardMarkup markup in telegramInlineKeyboardMarkup)
                        {
                            if (markup.CallBackData is not null && markup.CallBackData != "[\"null\"]")
                            {
                                string key = callBackDatas.Count.ToString();
                                callBackDatas.Add(key, markup.CallBackData);
                                markup.CallBackData = key;
                            }
                        }
                    }

                    string crypted = Helper.AesEncryptToBase64URL(JsonSerializer.Serialize(callBackDatas), apiKeys.GetSingle("TelegramAesKey"));

                    telegramSendMessage = new() { ChatId = telegramId, Text = $"[\u200b](tg://co/{crypted}){text}", ParseMode = "MarkdownV2", ReplyMarkup = new() };
                    telegramSendMessage.ReplyMarkup.InlineKeyboardMarkup = telegramInlineKeyboardMarkups;
                }

                else
                {
                    telegramSendMessage = new() { ChatId = telegramId, Text = text, ParseMode = "MarkdownV2" };
                }
            }

            else
            {
                if (telegramInlineKeyboardMarkups is not null)
                {
                    Dictionary<string, string> callBackDatas = [];

                    foreach (TelegramInlineKeyboardMarkup[] telegramInlineKeyboardMarkup in telegramInlineKeyboardMarkups)
                    {
                        foreach (TelegramInlineKeyboardMarkup markup in telegramInlineKeyboardMarkup)
                        {
                            if (markup.CallBackData is not null && markup.CallBackData != "[\"null\"]")
                            {
                                string key = callBackDatas.Count.ToString();
                                callBackDatas.Add(key, markup.CallBackData);
                                markup.CallBackData = key;
                            }
                        }
                    }

                    string crypted = Helper.AesEncryptToBase64URL(JsonSerializer.Serialize(callBackDatas), apiKeys.GetSingle("TelegramAesKey"));

                    telegramSendMessage = new() { ChatId = telegramId, Text = $"[\u200b](tg://co/{crypted}){languages.GetString(languageCode, text)}", ParseMode = "MarkdownV2", ReplyMarkup = new() };
                    telegramSendMessage.ReplyMarkup.InlineKeyboardMarkup = telegramInlineKeyboardMarkups;
                }

                else
                {
                    telegramSendMessage = new() { ChatId = telegramId, Text = languages.GetString(languageCode, text), ParseMode = "MarkdownV2" };
                }
            }

            if (replyMessageId.HasValue)
            {
                telegramSendMessage.ReplyParameters = new() { ChatId = telegramId, MessageId = (int)replyMessageId };
            }

            StringContent content = new(JsonSerializer.Serialize(telegramSendMessage, Helper.telegramSerializeOptions), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").PostAsync($"http://localhost:9595/bot{apiKeys.GetSingle("TelegramBotToken")}/sendMessage", content);
            string responseContent = await response.Content.ReadAsStringAsync();

            logger.LogInformation("Outcoming request: {request}", responseContent);
            TelegramReturnResult? telegramReturnResult = JsonSerializer.Deserialize<TelegramReturnResult>(responseContent);

            if (telegramReturnResult is not null && telegramReturnResult.Result is not null)
            {
                return telegramReturnResult.Result.MessageId;
            }

            return null;
        }

        public async Task EditMessage(long telegramId, int messageId, string text, TelegramInlineKeyboardMarkup[][]? telegramInlineKeyboardMarkups = null, bool isText = false, string? parseMode = "MarkdownV2", TelegramMessageEntity[]? telegramMessageEntities = null)
        {
            TelegramEditMessage telegramEditMessage;

            if (isText)
            {
                if (telegramInlineKeyboardMarkups is not null)
                {
                    Dictionary<string, string> callBackDatas = [];

                    foreach (TelegramInlineKeyboardMarkup[] telegramInlineKeyboardMarkup in telegramInlineKeyboardMarkups)
                    {
                        foreach (TelegramInlineKeyboardMarkup markup in telegramInlineKeyboardMarkup)
                        {
                            if (markup.CallBackData is not null && markup.CallBackData != "[\"null\"]")
                            {
                                string key = callBackDatas.Count.ToString();
                                callBackDatas.Add(key, markup.CallBackData);
                                markup.CallBackData = key;
                            }
                        }
                    }
                    string crypted = Helper.AesEncryptToBase64URL(JsonSerializer.Serialize(callBackDatas), apiKeys.GetSingle("TelegramAesKey"));
                    if (telegramMessageEntities is not null)
                    {
                        telegramMessageEntities[0].Url = $"tg://co/{crypted}";
                        telegramEditMessage = new() { ChatId = telegramId, MessageId = messageId, Text = text, ParseMode = parseMode, ReplyMarkup = new(), Entities = telegramMessageEntities };
                    }

                    else
                    {
                        telegramEditMessage = new() { ChatId = telegramId, MessageId = messageId, Text = $"[\u200b](tg://co/{crypted}){text}", ParseMode = parseMode, ReplyMarkup = new(), Entities = telegramMessageEntities };
                    }


                    telegramEditMessage.ReplyMarkup.InlineKeyboardMarkup = telegramInlineKeyboardMarkups;
                }

                else
                {
                    telegramEditMessage = new() { ChatId = telegramId, MessageId = messageId, Text = text, ParseMode = parseMode, Entities = telegramMessageEntities };
                }
            }

            else
            {
                if (telegramInlineKeyboardMarkups is not null)
                {
                    Dictionary<string, string> callBackDatas = [];

                    foreach (TelegramInlineKeyboardMarkup[] telegramInlineKeyboardMarkup in telegramInlineKeyboardMarkups)
                    {
                        foreach (TelegramInlineKeyboardMarkup markup in telegramInlineKeyboardMarkup)
                        {
                            if (markup.CallBackData is not null && markup.CallBackData != "[\"null\"]")
                            {
                                string key = callBackDatas.Count.ToString();
                                callBackDatas.Add(key, markup.CallBackData);
                                markup.CallBackData = key;
                            }
                        }
                    }

                    string crypted = Helper.AesEncryptToBase64URL(JsonSerializer.Serialize(callBackDatas), apiKeys.GetSingle("TelegramAesKey"));

                    if (telegramMessageEntities is not null)
                    {
                        telegramMessageEntities[0].Url = $"tg://co/{crypted}";
                        telegramEditMessage = new() { ChatId = telegramId, MessageId = messageId, Text = languages.GetString(await userService.GetUserLanguageCode(telegramId), text), ParseMode = parseMode, ReplyMarkup = new(), Entities = telegramMessageEntities };
                    }

                    else
                    {
                        telegramEditMessage = new() { ChatId = telegramId, MessageId = messageId, Text = $"[\u200b](tg://co/{crypted}){languages.GetString(await userService.GetUserLanguageCode(telegramId), text)}", ParseMode = parseMode, ReplyMarkup = new(), Entities = telegramMessageEntities };
                    }

                    telegramEditMessage.ReplyMarkup.InlineKeyboardMarkup = telegramInlineKeyboardMarkups;
                }

                else
                {
                    telegramEditMessage = new() { ChatId = telegramId, MessageId = messageId, Text = languages.GetString(await userService.GetUserLanguageCode(telegramId), text), ParseMode = parseMode, Entities = telegramMessageEntities };
                }
            }

            StringContent content = new(JsonSerializer.Serialize(telegramEditMessage, Helper.telegramSerializeOptions), Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").PostAsync($"http://localhost:9595/bot{apiKeys.GetSingle("TelegramBotToken")}/editMessageText", content);
                string responseContent = await response.Content.ReadAsStringAsync();

                logger.LogInformation("Outcoming request: {request}", responseContent);
                TelegramReturnResult? telegramReturnResult = JsonSerializer.Deserialize<TelegramReturnResult>(responseContent);

                if (telegramReturnResult is not null && telegramReturnResult.Ok == false && telegramReturnResult.Description == "Bad Request: message is not modified: specified new message content and reply markup are exactly the same as a current content and reply markup of the message")
                {

                }

                else if (!response.IsSuccessStatusCode)
                {
                    if (telegramReturnResult is not null)
                    {
                        throw new Exception($"Status code error TELEGRAM: {response.StatusCode} Content: {telegramReturnResult.Description}");
                    }

                    else
                    {
                        throw new Exception($"Status code error TELEGRAM: {response.StatusCode}");
                    }
                }
            }

            catch (Exception ex)
            {
                logger.LogWarning(ex, "Telegram editMessage error");
                //await SendMessage(telegramId, text, telegramInlineKeyboardMarkups, null, isText, null);
            }
        }

        public async Task SetMessageReaction(long telegramId, int messageId, string? emoji = null)
        {
            TelegramSetMessageReaction setMessageReaction = new() { ChatId = telegramId, MessageId = messageId, Reaction = !String.IsNullOrWhiteSpace(emoji) ? [new() { Emoji = emoji }] : [] };

            StringContent content = new(JsonSerializer.Serialize(setMessageReaction, Helper.telegramSerializeOptions), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").PostAsync($"http://localhost:9595/bot{apiKeys.GetSingle("TelegramBotToken")}/setMessageReaction", content);
            string responseContent = await response.Content.ReadAsStringAsync();

            logger.LogInformation("Outcoming request: {request}", responseContent);
        }

        public async Task AnswerCallbackQuery(string callBackQueryId, string text = "", bool sendAsAlert = false, int? cacheTime = null)
        {
            TelegramAnswerCallbackQuery telegramAnswerCallbackQuery = new() { CallbackQueryId = callBackQueryId, Text = text, ShowAlert = sendAsAlert, CacheTime = cacheTime };
            StringContent content = new(JsonSerializer.Serialize(telegramAnswerCallbackQuery), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").PostAsync($"http://localhost:9595/bot{apiKeys.GetSingle("TelegramBotToken")}/answerCallbackQuery", content);
            string responseContent = await response.Content.ReadAsStringAsync();

            logger.LogInformation("Outcoming request: {request}", responseContent);
        }
    }
}