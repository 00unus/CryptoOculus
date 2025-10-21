using CryptoOculus.Models;

namespace CryptoOculus
{
    public interface IExchange
    {
        public int ExchangeId { get; }
        public string ExchangeName { get; }
        public Task<Pair[]> GetPairs();
        public Task<double[][]?> OrderBook(string baseAsset, string quoteAsset, bool askOrBid);
    }
}
