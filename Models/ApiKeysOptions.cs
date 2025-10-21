using System.Collections.Concurrent;

namespace CryptoOculus.Models
{
    public class ApiKeysOptions
    {
        public required ConcurrentDictionary<string, string> SingleKeys { get; set; }
        public required ConcurrentDictionary<string, string[]> MultipleKeys { get; set; }
    }
}
