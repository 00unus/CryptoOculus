using System.Threading.RateLimiting;

namespace CryptoOculus
{
    public static class HttpOptionKeys
    {
        public static readonly HttpRequestOptionsKey<string> RequestBody = new("RequestBody");
        public static readonly HttpRequestOptionsKey<string> ResponseBody = new("ResponseBody");
        public static readonly HttpRequestOptionsKey<int> StatusCode = new("StatusCode");
        public static readonly HttpRequestOptionsKey<Uri> RequestUri = new("RequestUri");
        public static readonly HttpRequestOptionsKey<Action<HttpRequestMessage>> ValidationDelegate = new("ValidationDelegate");
        public static readonly HttpRequestOptionsKey<string> ClientName = new("ClientName");
        public static readonly HttpRequestOptionsKey<string> LimitRuleName = new("LimitRuleName");
        public static readonly HttpRequestOptionsKey<string> KeyedLimitRuleName = new("KeyedLimitRuleName");
        public static readonly HttpRequestOptionsKey<string> KeyedLimitRuleKey = new("KeyedLimitRuleKey");
        public static readonly HttpRequestOptionsKey<string> ConcurrencyLimitName = new("ConcurrencyLimitName");
        public static readonly HttpRequestOptionsKey<string> KeyedConcurrencyLimitName = new("KeyedConcurrencyLimitName");
        public static readonly HttpRequestOptionsKey<string> KeyedConcurrencyLimitKey = new("KeyedConcurrencyLimitKey");
        public static readonly HttpRequestOptionsKey<List<RateLimitLease>> RateLimitLeases = new("RateLimitLeases");
    }
}
