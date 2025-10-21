using System.Text.Json.Serialization;

namespace CryptoOculus.Models
{
    public class TelegramUpdate
    {
        [JsonPropertyName("update_id")]
        public required int UpdateId { get; set; }

        [JsonPropertyName("message")]
        public TelegramMessage? Message { get; set; }

        [JsonPropertyName("callback_query")]
        public TelegramCallbackQuery? CallbackQuery { get; set; }
    }

    public class TelegramMessage
    {
        [JsonPropertyName("message_id")]
        public required int MessageId { get; set; }

        [JsonPropertyName("from")]
        public TelegramUser? From { get; set; }

        [JsonPropertyName("chat")]
        public TelegramChat? Chat { get; set; }

        [JsonPropertyName("date")]
        public required long Date { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("reply_markup")]
        public TelegramReplyMarkup? ReplyMarkup { get; set; }

        [JsonPropertyName("entities")]
        public TelegramMessageEntity[]? Entities { get; set; }
    }
    public class TelegramUser
    {
        [JsonPropertyName("id")]
        public required long Id { get; set; }

        [JsonPropertyName("is_bot")]
        public required bool IsBot { get; set; }

        [JsonPropertyName("first_name")]
        public required string FirstName { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("language_code")]
        public string? LanguageCode { get; set; }
    }
    public class TelegramChat
    {
        [JsonPropertyName("id")]
        public required long Id { get; set; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("type")]
        public required string Type { get; set; }
    }
    public class TelegramMessageEntity
    {
        [JsonPropertyName("offset")]
        public required int Offset { get; set; }

        [JsonPropertyName("length")]
        public required int Length { get; set; }

        [JsonPropertyName("type")]
        public required string Type { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }

    public class TelegramCallbackQuery
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonPropertyName("from")]
        public required TelegramUser From { get; set; }

        [JsonPropertyName("message")]
        public TelegramMessage? Message { get; set; }

        [JsonPropertyName("inline_message_id")]
        public string? InlineMessageId { get; set; }

        [JsonPropertyName("chat_instance")]
        public string? ChatInstance { get; set; }

        [JsonPropertyName("data")]
        public string? Data { get; set; }

        [JsonPropertyName("game_short_name")]
        public string? GameShortName { get; set; }
    }


    public class TelegramReturnResult
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("result")]
        public TelegramMessage? Result { get; set; }

        [JsonPropertyName("error_code")]
        public int? ErrorCode { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }


    public class TelegramSendMessage
    {
        [JsonPropertyName("chat_id")]
        public required long ChatId { get; set; }

        [JsonPropertyName("text")]
        public required string Text { get; set; }

        [JsonPropertyName("parse_mode")]
        public string? ParseMode { get; set; }

        [JsonPropertyName("link_preview_options")]
        public TelegramLinkPreviewOptions LinkPreviewOptions { get; set; } = new() { IsDisabled = true };

        [JsonPropertyName("reply_parameters")]
        public TelegramReplyParameters? ReplyParameters { get; set; }

        [JsonPropertyName("reply_markup")]
        public TelegramReplyMarkup? ReplyMarkup { get; set; }
    }
    public class TelegramLinkPreviewOptions
    {
        [JsonPropertyName("is_disabled")]
        public bool IsDisabled { get; set; }
    }
    public class TelegramReplyParameters
    {
        [JsonPropertyName("message_id")]
        public required int MessageId { get; set; }

        [JsonPropertyName("chat_id")]
        public required long ChatId { get; set; }

        [JsonPropertyName("allow_sending_without_reply")]
        public bool AllowSendingWithoutReply { get; set; } = true;
    }
    public class TelegramReplyMarkup
    {
        [JsonPropertyName("inline_keyboard")]
        public TelegramInlineKeyboardMarkup[][]? InlineKeyboardMarkup { get; set; }
    }
    public class TelegramInlineKeyboardMarkup
    {
        [JsonPropertyName("text")]
        public required string Text { get; set; }

        [JsonPropertyName("callback_data")]
        public string? CallBackData { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }


    public class TelegramEditMessage
    {
        [JsonPropertyName("chat_id")]
        public required long ChatId { get; set; }

        [JsonPropertyName("message_id")]
        public required int MessageId { get; set; }

        [JsonPropertyName("text")]
        public required string Text { get; set; }

        [JsonPropertyName("parse_mode")]
        public string? ParseMode { get; set; }

        [JsonPropertyName("link_preview_options")]
        public TelegramLinkPreviewOptions LinkPreviewOptions { get; set; } = new() { IsDisabled = true };

        [JsonPropertyName("reply_markup")]
        public TelegramReplyMarkup? ReplyMarkup { get; set; }

        [JsonPropertyName("entities")]
        public TelegramMessageEntity[]? Entities { get; set; }
    }


    public class TelegramAnswerCallbackQuery
    {
        [JsonPropertyName("callback_query_id")]
        public required string CallbackQueryId { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("show_alert")]
        public bool? ShowAlert { get; set; }

        [JsonPropertyName("cache_time")]
        public int? CacheTime { get; set; }
    }


    public class TelegramSetMessageReaction
    {
        [JsonPropertyName("chat_id")]
        public required long ChatId { get; set; }

        [JsonPropertyName("message_id")]
        public required int MessageId { get; set; }

        [JsonPropertyName("reaction")]
        public required TelegramReactionType[] Reaction { get; set; }

        [JsonPropertyName("is_big")]
        public bool IsBig { get; set; } = true;
    }
    public class TelegramReactionType
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "emoji";

        [JsonPropertyName("emoji")]
        public required string Emoji { get; set; }
    }
}
