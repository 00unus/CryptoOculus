namespace CryptoOculus.Models
{
    public class UserSettings
    {
        public required long TelegramId { get; set; }
        public required string LanguageCode { get; set; }
        public required double Profit { get; set; }
        public required double Amount { get; set; }
        public required long LastActionDate { get; set; }
        public required long NewSpreadsDelay { get; set; }
        public required string BuyExBlacklist { get; set; }
        public required string SellExBlacklist { get; set; }
        public required string SpreadsBlacklist { get; set; }
        public required string CoinsBlacklist { get; set; }
        public string? SubscribedItem { get; set; }
    }
}
