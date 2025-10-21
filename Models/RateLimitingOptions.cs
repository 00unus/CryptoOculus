using System.Collections.Concurrent;

namespace CryptoOculus.Models
{
    public class RateLimitingOptions
    {
        public required ConcurrentDictionary<string, FixedWindowRule> EndpointRateLimit { get; set; }
        public required DomainWeightOptions DomainWeight { get; set; }
        public required RuleLimitOptions[] RuleLimits { get; set; }
        public required ConcurrencyLimitOptions[] ConcurrencyLimits { get; set; }
        public required ProxyGroupOptions[] ProxyGroups { get; set; }
    }
    public class FixedWindowRule
    {
        public int Limit { get; set; }
        public int WindowSeconds { get; set; }
    }
    public class DomainWeightOptions
    {
        public required Dictionary<string, FixedWindowRule> DomainLimits { get; set; }
        public required Dictionary<string, int> EndpointWeights { get; set; }
    }
    public class RuleLimitOptions
    {
        public required string Name { get; set; }
        public int Limit { get; set; }
        public double WindowSeconds { get; set; }
    }
    public class ConcurrencyLimitOptions
    {
        public required string Name { get; set; }
        public int MaxConcurrency { get; set; }
    }
    public class ProxyGroupOptions
    {
        public required string ProxyIds { get; set; }
        public int MaxConcurrency { get; set; }
    }
}