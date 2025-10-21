namespace CryptoOculus.Models
{
    public class SubscribeDetails
    {
        public required string Type { get; set; }
        public int MessageId { get; set; } = -1;
        public string? SpreadId { get; set; }
        public int? RequiredPage { get; set; }
        public string? InnerSpreadsId { get; set; }
        public string? BackString { get; set; }
    }
}
