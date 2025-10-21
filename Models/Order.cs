namespace CryptoOculus.Models
{
    public class Order
    {
        public required double Price { get; set; }
        public required double Quantity { get; set; }
        public required double Amount { get; set; }
        public required double[] PriceRange { get; set; }
    }
}
