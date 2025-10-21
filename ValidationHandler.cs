using System.Security.Cryptography;
using System.Text;

namespace CryptoOculus
{
    public class ValidationHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            /*List<KeyValuePair<string, IEnumerable<string>>> headers = [];

            foreach (var header in request.Headers)
            {
                headers.Add(new KeyValuePair<string, IEnumerable<string>>(header.Key, header.Value));
            }

            if (request.Content != null)
            {
                foreach (var header in request.Content.Headers)
                    headers.Add(new KeyValuePair<string, IEnumerable<string>>(header.Key, header.Value));
            }

            request.Headers.Clear();
            request.Content?.Headers.Clear();

            headers = [.. headers.OrderBy(x => RandomNumberGenerator.GetInt32(int.MaxValue))];

            // Добавляем обратно
            foreach (var header in headers)
            {
                if (!request.Headers.TryAddWithoutValidation(header.Key, [.. header.Value]))
                {
                    // если не получилось в обычные заголовки — пробуем в Content.Headers
                    request.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }*/

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

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

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
