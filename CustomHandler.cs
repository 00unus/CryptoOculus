using CryptoOculus.Services;
using System.Text;
using System.Threading.RateLimiting;

namespace CryptoOculus
{
    public class CustomHandler(RateLimiterManager rateLimiterManager) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //Save URI of request
            if (request.RequestUri is not null)
            {
                request.Options.Set(HttpOptionKeys.RequestUri, request.RequestUri);
            }

            //Save request body
            if (request.Content is not null)
            {
                byte[] byteArray = await request.Content.ReadAsByteArrayAsync(cancellationToken);

                ByteArrayContent byteArrayContent = new(byteArray);
                foreach (var header in request.Content.Headers)
                {
                    byteArrayContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                request.Content = byteArrayContent;

                request.Options.Set(HttpOptionKeys.RequestBody, Encoding.UTF8.GetString(byteArray));
            }

            await rateLimiterManager.WaitAsync(request, cancellationToken);

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            if (request.Options.TryGetValue(HttpOptionKeys.RateLimitLeases, out List<RateLimitLease>? leases))
            {
                foreach (RateLimitLease lease in leases)
                {
                    lease.Dispose();
                }
            }

            //Save status code
            request.Options.Set(HttpOptionKeys.StatusCode, (int)response.StatusCode);

            //Save response body
            if (response.Content is not null)
            {
                byte[] byteArray = await response.Content.ReadAsByteArrayAsync(cancellationToken);

                ByteArrayContent byteArrayContent = new(byteArray);
                foreach (var header in response.Content.Headers)
                {
                    byteArrayContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                response.Content = byteArrayContent;

                request.Options.Set(HttpOptionKeys.ResponseBody, Encoding.UTF8.GetString(byteArray));
            }

            //Validation
            if (response.IsSuccessStatusCode && request.Options.TryGetValue(HttpOptionKeys.ValidationDelegate, out Action<HttpRequestMessage>? validationDelegate))
            {
                validationDelegate(request);
            }

            return response;
        }
    }
}