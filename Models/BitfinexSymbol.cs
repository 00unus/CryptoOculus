namespace CryptoOculus.Models
{
    public class BitfinexSymbol
    {
        public required string Symbol { get; set; }
        public required string BaseAsset { get; set; }
        public required string QuoteAsset { get; set; }
    }
}
