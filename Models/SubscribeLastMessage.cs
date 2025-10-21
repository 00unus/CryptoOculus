namespace CryptoOculus.Models
{
    public class SubscribeLastMessage
    {
        public required string Text { get; set; }

        public required TelegramInlineKeyboardMarkup[][] InlineKeyboardMarkups { get; set; }
    }
}
