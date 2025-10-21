namespace CryptoOculus.Models
{
    public class NetworkList
    {
        public required int Id { get; set; }
        public required string NetworkName { get; set; }
        public required string[] CEXs { get; set; }
    }
}
