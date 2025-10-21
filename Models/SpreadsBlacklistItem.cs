namespace CryptoOculus.Models
{
    public class SpreadsBlacklistItem
    {
        public required DateTimeOffset Date { get; set; }
        public required int BuyExchangeId { get; set; }
        public required int SellExchangeId { get; set; }
        public required string BuyExchangePairBaseAsset { get; set; }
        public required string BuyExchangePairQuoteAsset { get; set; }
        public required string SellExchangePairBaseAsset { get; set; }
        public required string SellExchangePairQuoteAsset { get; set; }
    }
}