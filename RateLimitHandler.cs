using CryptoOculus.Services;
using System.Threading.RateLimiting;

namespace CryptoOculus
{
    public class RateLimitHandler(RateLimiterManager rateLimiterManager) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                await rateLimiterManager.WaitAsync(request, cancellationToken);

                return await base.SendAsync(request, cancellationToken);
            }
            catch { throw; }
            finally
            {
                if (request.Options.TryGetValue(HttpOptionKeys.RateLimitLeases, out List<RateLimitLease>? leases))
                {
                    foreach (RateLimitLease lease in leases)
                    {
                        lease.Dispose();
                    }
                }
            }
        }
    }
}
