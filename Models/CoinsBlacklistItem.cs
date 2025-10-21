namespace CryptoOculus.Models
{
    public class CoinsBlacklistItem
    {
        public required DateTimeOffset Date { get; set; }
        public int? ExchangeId { get; set; }
        public required string BaseAsset { get; set; }
    }
}