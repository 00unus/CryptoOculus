using CryptoOculus.Models;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Threading.RateLimiting;

namespace CryptoOculus.Services
{
    public class RateLimiterManager
    {
        private readonly RateLimitingOptions _rateLimitingOptions;

        private static readonly ConcurrentDictionary<string, RateLimiter> _endpointLimiters = new();
        private static readonly ConcurrentDictionary<string, RateLimiter> _domainWeightLimiters = new();
        private static readonly ConcurrentDictionary<string, RateLimiter> _limitRuleLimiters = new();
        private static readonly ConcurrentDictionary<(string RuleName, string Key), RateLimiter> _keyedLimitRuleLimiters = new();
        private static readonly ConcurrentDictionary<string, ConcurrencyLimiter> _concurrencyLimitLimiters = new();
        private static readonly ConcurrentDictionary<(string RuleName, string Key), ConcurrencyLimiter> _keyedConcurrencyLimitLimiters = new();
        private static readonly ConcurrentDictionary<string, ConcurrencyLimiter> _proxyGroupLimiters = new();

        public RateLimiterManager(IOptions<RateLimitingOptions> rateLimitingOptions)
        {
            _rateLimitingOptions = rateLimitingOptions.Value;

            foreach (ProxyGroupOptions group in _rateLimitingOptions.ProxyGroups)
            {
                ConcurrencyLimiter limiter = new(new ConcurrencyLimiterOptions
                {
                    PermitLimit = group.MaxConcurrency,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = int.MaxValue
                });

                List<string> parsedProxyIds = [];

                foreach (string part in group.ProxyIds.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    string trimmed = part.Trim();

                    if (trimmed.Contains('-'))
                    {
                        string[] range = trimmed.Split('-', 2);
                        if (int.TryParse(range[0], out int start) && int.TryParse(range[1], out int end))
                        {
                            for (int i = start; i <= end; i++)
                            {
                                parsedProxyIds.Add(i.ToString());
                            }
                        }
                    }
                    else
                    {
                        parsedProxyIds.Add(trimmed);
                    }
                }

                foreach (string proxyId in parsedProxyIds)
                {
                    _proxyGroupLimiters[proxyId] = limiter;
                }
            }
        }

        public async ValueTask WaitAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            List<RateLimitLease> leases = [];

            try
            {
                string clientName = request.Options.TryGetValue(HttpOptionKeys.ClientName, out string? tempClientName) ? tempClientName : "Standard";
                string endpoint = (request.Headers.Host ?? request.RequestUri!.Host) + request.RequestUri!.AbsolutePath;
                string domain = request.Headers.Host ?? request.RequestUri!.Host;

                foreach (KeyValuePair<string, FixedWindowRule> endpointRateLimit in _rateLimitingOptions.EndpointRateLimit)
                {
                    if (endpoint.StartsWith(endpointRateLimit.Key.Split("{{{...}}}")[0]))
                    {
                        RateLimiter limiter = _endpointLimiters.GetOrAdd($"{endpointRateLimit.Key}:{clientName}", _ =>
                        new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = endpointRateLimit.Value.Limit,
                            Window = TimeSpan.FromSeconds(endpointRateLimit.Value.WindowSeconds),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = int.MaxValue
                        }));

                        await limiter.AcquireAsync(1, cancellationToken);
                        break;
                    }
                }
                /*if (_rateLimitingOptions.EndpointRateLimit.TryGetValue(endpoint, out FixedWindowRule? endpointRule))
                {
                    RateLimiter limiter = _endpointLimiters.GetOrAdd($"{endpoint}:{clientName}", _ =>
                        new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = endpointRule.Limit,
                            Window = TimeSpan.FromSeconds(endpointRule.WindowSeconds),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = int.MaxValue
                        }));

                    await limiter.AcquireAsync(1, linkedCts.Token);
                }*/

                if (_rateLimitingOptions.DomainWeight.DomainLimits.TryGetValue(domain, out FixedWindowRule? domainRule) &&
                    _rateLimitingOptions.DomainWeight.EndpointWeights.TryGetValue(endpoint, out int weight))
                {
                    RateLimiter limiter = _domainWeightLimiters.GetOrAdd($"{domain}:{clientName}", _ =>
                        new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = domainRule.Limit,
                            Window = TimeSpan.FromSeconds(domainRule.WindowSeconds),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = int.MaxValue
                        }));

                    await limiter.AcquireAsync(weight, cancellationToken);
                }

                if (request.Options.TryGetValue(HttpOptionKeys.LimitRuleName, out string? limitRuleName))
                {
                    RuleLimitOptions? rule = _rateLimitingOptions.RuleLimits.FirstOrDefault(x => x.Name == limitRuleName);

                    if (rule is not null)
                    {
                        RateLimiter limiter = _limitRuleLimiters.GetOrAdd($"{limitRuleName}:{clientName}", _ =>
                            new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = rule.Limit,
                                Window = TimeSpan.FromSeconds(rule.WindowSeconds),
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                QueueLimit = int.MaxValue
                            }));

                        await limiter.AcquireAsync(1, cancellationToken);
                    }
                }

                if (request.Options.TryGetValue(HttpOptionKeys.KeyedLimitRuleName, out string? keyedRuleName) &&
                    request.Options.TryGetValue(HttpOptionKeys.KeyedLimitRuleKey, out string? keyedLimitKey))
                {
                    RuleLimitOptions? rule = _rateLimitingOptions.RuleLimits.FirstOrDefault(x => x.Name == keyedRuleName);

                    if (rule is not null)
                    {
                        RateLimiter limiter = _keyedLimitRuleLimiters.GetOrAdd((rule.Name, keyedLimitKey), _ =>
                            new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = rule.Limit,
                                Window = TimeSpan.FromSeconds(rule.WindowSeconds),
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                QueueLimit = int.MaxValue
                            }));

                        await limiter.AcquireAsync(1, cancellationToken);
                    }
                }

                if (request.Options.TryGetValue(HttpOptionKeys.ConcurrencyLimitName, out string? concurrencyLimitName))
                {
                    ConcurrencyLimitOptions? concurrencyLimit = _rateLimitingOptions.ConcurrencyLimits.FirstOrDefault(x => x.Name == concurrencyLimitName);

                    if (concurrencyLimit is not null)
                    {
                        ConcurrencyLimiter limiter = _concurrencyLimitLimiters.GetOrAdd($"{concurrencyLimitName}:{clientName}", _ =>
                            new(new ConcurrencyLimiterOptions
                            {
                                PermitLimit = concurrencyLimit.MaxConcurrency,
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                QueueLimit = int.MaxValue
                            }));

                        leases.Add(await limiter.AcquireAsync(1, cancellationToken));
                    }
                }

                if (request.Options.TryGetValue(HttpOptionKeys.KeyedConcurrencyLimitName, out string? keyedConcurrencyLimitName) &&
                    request.Options.TryGetValue(HttpOptionKeys.KeyedConcurrencyLimitKey, out string? keyedConcurrencyLimitKey))
                {
                    ConcurrencyLimitOptions? concurrencyLimit = _rateLimitingOptions.ConcurrencyLimits.FirstOrDefault(x => x.Name == keyedConcurrencyLimitName);

                    if (concurrencyLimit is not null)
                    {
                        ConcurrencyLimiter limiter = _keyedConcurrencyLimitLimiters.GetOrAdd((keyedConcurrencyLimitName, keyedConcurrencyLimitKey), _ =>
                            new(new ConcurrencyLimiterOptions
                            {
                                PermitLimit = concurrencyLimit.MaxConcurrency,
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                QueueLimit = int.MaxValue
                            }));

                        leases.Add(await limiter.AcquireAsync(1, cancellationToken));
                    }
                }

                if (_proxyGroupLimiters.TryGetValue(clientName, out ConcurrencyLimiter? concurrencyLimiter))
                {
                    leases.Add(await concurrencyLimiter.AcquireAsync(1, cancellationToken));
                }

                if (leases.Count > 0)
                {
                    request.Options.Set(HttpOptionKeys.RateLimitLeases, leases);
                }
            }
            catch
            {
                foreach (RateLimitLease lease in leases)
                {
                    lease.Dispose();
                }

                throw;
            }
        }
    }
}