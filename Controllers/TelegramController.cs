using CryptoOculus.Models;
using CryptoOculus.Services;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
namespace CryptoOculus.Controllers
{
    [ApiController]
    public class TelegramController(ApiKeysService apiKeys, ILogger<TelegramController> logger) : ControllerBase
    {
        [HttpPost]
        [Route("webhook")]
        public async Task<IActionResult> Webhook([FromBody] TelegramUpdate telegramUpdate, TelegramService telegramService, UserService userService)
        {
            logger.LogInformation("Incoming request: {request}", JsonSerializer.Serialize<TelegramUpdate>(telegramUpdate).ToString());

            try
            {
                //Command
                if (telegramUpdate.Message is not null && telegramUpdate.Message.Chat is not null && telegramUpdate.Message.Chat.Type == "private" && telegramUpdate.Message.From is not null && !telegramUpdate.Message.From.IsBot)
                {
                    string[] waitingMethods = [];

                    if (await userService.UpdateUserLastActionDate(telegramUpdate.Message.From.Id, DateTimeOffset.UtcNow.ToUnixTimeSeconds()))
                    {
                        string? waitingMethodsJson = await userService.GetUserWaitingMethods(telegramUpdate.Message.From.Id);

                        if (!String.IsNullOrWhiteSpace(waitingMethodsJson))
                        {
                            waitingMethods = JsonSerializer.Deserialize<string[]>(waitingMethodsJson)!;
                            await userService.UpdateUserWaitingMethods(telegramUpdate.Message.From.Id, null);
                        }
                    }

                    if (telegramUpdate.Message.Entities is not null && telegramUpdate.Message.Entities[0].Type == "bot_command" && telegramUpdate.Message.From.LanguageCode is not null)
                    {
                        switch (telegramUpdate.Message.Text)
                        {
                            case "/start":
                                await telegramService.StartCommand(telegramUpdate.Message.From.Id, telegramUpdate.Message.From.LanguageCode, telegramUpdate.Message.MessageId);
                                break;

                            case "/settings":
                                await telegramService.SettingsCommand(telegramUpdate.Message.From.Id, telegramUpdate.Message.From.LanguageCode, telegramUpdate.Message.MessageId);
                                break;

                            case "/spreads":
                                await telegramService.SpreadsCommand(telegramUpdate.Message.From.Id, telegramUpdate.Message.From.LanguageCode, telegramUpdate.Message.MessageId);
                                break;

                            default:
                                await telegramService.StartCommand(telegramUpdate.Message.From.Id, telegramUpdate.Message.From.LanguageCode!, telegramUpdate.Message.MessageId);
                                break;
                        }
                    }

                    else
                    {
                        foreach (string waitingMethod in waitingMethods)
                        {
                            await telegramService.Methods(waitingMethod, telegramUpdate.Message.From.Id, null, null, telegramUpdate.Message.Text, telegramUpdate.Message.MessageId);
                        }
                    }
                }

                //Callback query (Button click)
                else if (telegramUpdate.CallbackQuery is not null && telegramUpdate.CallbackQuery.Data is not null &&
                         telegramUpdate.CallbackQuery.Message is not null && telegramUpdate.CallbackQuery.Message.Chat is not null &&
                         telegramUpdate.CallbackQuery.Message.Chat.Type == "private")
                {
                    if (telegramUpdate.CallbackQuery.Data == "null")
                    {
                        await telegramService.AnswerCallbackQuery(telegramUpdate.CallbackQuery.Id);
                    }

                    else if (telegramUpdate.CallbackQuery.Message.Entities is not null && telegramUpdate.CallbackQuery.Message.Entities.Length > 0 &&
                        telegramUpdate.CallbackQuery.Message.Entities[0].Url is not null && telegramUpdate.CallbackQuery.Message.Entities[0].Url!.StartsWith("tg://co/"))
                    {
                        if (await userService.UpdateUserLastActionDate(telegramUpdate.CallbackQuery.From.Id, DateTimeOffset.UtcNow.ToUnixTimeSeconds()))
                        {
                            await userService.UpdateUserWaitingMethods(telegramUpdate.CallbackQuery.From.Id, null);
                        }

                        string[] methodNames = [];

                        try
                        {
                            Dictionary<string, string> callBackDatas = JsonSerializer.Deserialize<Dictionary<string, string>>(Helper.AesDecryptFromBase64URL(telegramUpdate.CallbackQuery.Message.Entities[0].Url!.Replace("tg://co/", String.Empty), apiKeys.GetSingle("TelegramAesKey")))!;
                            methodNames = callBackDatas.TryGetValue(telegramUpdate.CallbackQuery.Data, out string? data) ? JsonSerializer.Deserialize<string[]>(data) ?? [] : [];

                            if (telegramUpdate.CallbackQuery.Message.ReplyMarkup?.InlineKeyboardMarkup is not null)
                            {
                                foreach (TelegramInlineKeyboardMarkup[] markups in telegramUpdate.CallbackQuery.Message.ReplyMarkup.InlineKeyboardMarkup)
                                {
                                    foreach (TelegramInlineKeyboardMarkup markup in markups)
                                    {
                                        if (!String.IsNullOrWhiteSpace(markup.CallBackData) && callBackDatas.TryGetValue(markup.CallBackData, out string? callBackData))
                                        {
                                            markup.CallBackData = callBackData;
                                        }
                                    }
                                }
                            }
                        }
                        catch { }
                        Console.WriteLine(methodNames[0]);
                        foreach (string methodName in methodNames)
                        {
                            await telegramService.Methods(methodName, telegramUpdate.CallbackQuery.From.Id, telegramUpdate.CallbackQuery.Message.MessageId, telegramUpdate.CallbackQuery.Id, messageText: telegramUpdate.CallbackQuery.Message.Text, messageInlineKeyboard: telegramUpdate.CallbackQuery.Message.ReplyMarkup?.InlineKeyboardMarkup, telegramMessageEntities: telegramUpdate.CallbackQuery.Message.Entities);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ex.Message}", ex.Message);
            }

            return Ok();
        }
    }
}