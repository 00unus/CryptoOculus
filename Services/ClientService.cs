using Polly;
using Polly.Retry;
using Polly.Timeout;
using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class ClientService
    {
        private static readonly ConcurrentDictionary<string, int> _rotatingProxyIndexes = new();
        private static int proxyCount;

        public WebApplicationBuilder ConfigurateClientsAsync(WebApplicationBuilder builder, string[] proxyLines)
        {
            /*var fluxzyStartupSetting = FluxzySetting.CreateLocalRandomPort();
            // Mandatory, BouncyCastle must be used to reproduce the fingerprints
            fluxzyStartupSetting.UseBouncyCastleSslEngine();

            // Add an impersonation rule for Chrome 131
            fluxzyStartupSetting.AddAlterationRulesForAny(
                new ImpersonateAction(ImpersonateProfileManager.Edge131Windows));

            // Create a proxy instance
            await using Proxy proxy = new(fluxzyStartupSetting);
            IReadOnlyCollection<IPEndPoint> endpoints = proxy.Run();

            await using IAsyncDisposable proxyRegistration = await SystemProxyRegistrationHelper.Create(endpoints.First());

            // Fluxzy is now registered as the system proxy, the proxy will revert
            // back to the original settings when proxyRegistration is disposed.

            Console.WriteLine("Press any key to halt proxy and unregistered");

            Console.ReadKey();*/

            /*FluxzyDefaultHandler handler = new(SslProvider.BouncyCastle, ITcpConnectionProvider.Default, new EventOnlyArchiveWriter());

            ImpersonateConfiguration configuration = ImpersonateConfigurationManager.Instance.LoadConfiguration(ImpersonateProfileManager.Edge131Windows)!;

            TlsFingerPrint fingerPrint = TlsFingerPrint.ParseFromJa3(
             configuration.NetworkSettings.Ja3FingerPrint,
             configuration.NetworkSettings.GreaseMode,
             signatureAndHashAlgorithms: configuration.NetworkSettings.SignatureAlgorithms?.Select(s => SignatureAndHashAlgorithm.GetInstance(SignatureScheme.GetHashAlgorithm(s), SignatureScheme.GetSignatureAlgorithm(s))).ToList(),
             earlyShardGroups: configuration.NetworkSettings.EarlySharedGroups);

            handler.ConfigureContext = (exchangeContext) =>
            {
                exchangeContext.AdvancedTlsSettings.TlsFingerPrint = fingerPrint;
                exchangeContext.AdvancedTlsSettings.H2StreamSetting = configuration.H2Settings.ToH2StreamSetting();
            };
            handler.ConfigureContext = (exchangeContext) =>
            {
                exchangeContext.AdvancedTlsSettings.TlsFingerPrint = TlsFingerPrint.ParseFromImpersonateConfiguration(configuration);
                exchangeContext.AdvancedTlsSettings.H2StreamSetting = configuration.H2Settings.ToH2StreamSetting();
                exchangeContext.RequestHeaderAlterations.Add(new HeaderAlterationReplace("User-Agent", "My Custom Hook", true));
            };

            using HttpClient httpClient = new(handler) { Timeout = TimeSpan.FromSeconds(10) };

            // make request
            HttpRequestMessage requestMessage = new(HttpMethod.Get, "https://tls.peet.ws/api/all");
            requestMessage.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/139.0.0.0 Safari/537.36");
            HttpResponseMessage response = httpClient.SendAsync(requestMessage).Result;

            Console.WriteLine(response.Content.ReadAsStringAsync().Result);*/

            //Loading proxies
            List<WebProxy> proxyList = [];

            for (int i = 0; i < proxyLines.Length; i++)
            {
                Uri uri = new(proxyLines[i]);

                NetworkCredential? networkCredential = null;
                if (!String.IsNullOrWhiteSpace(uri.UserInfo))
                {
                    string[] userInfo = uri.UserInfo.Split(":", 2);
                    networkCredential = new(Uri.UnescapeDataString(userInfo[0]), Uri.UnescapeDataString(userInfo[1]));
                }

                proxyList.Add(new WebProxy()
                {
                    Address = uri,
                    Credentials = networkCredential
                });
            }

            proxyCount = proxyList.Count;

            //Adding standard client
            IHttpClientBuilder standardHttpClientBuilder = builder.Services.AddHttpClient("Standard", client =>
            {
                client.DefaultRequestVersion = HttpVersion.Version30;
                client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
                client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
                client.DefaultRequestHeaders.Connection.Add("keep-alive");
                client.Timeout = Timeout.InfiniteTimeSpan;
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
            {
                AutomaticDecompression = DecompressionMethods.All,
                UseCookies = false
            });
            standardHttpClientBuilder.AddHttpMessageHandler<RateLimitHandler>();
            standardHttpClientBuilder.AddResilienceHandler("pipeline", (builder, context) =>
            {
                builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromMilliseconds(300),
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .HandleResult(r => !r.IsSuccessStatusCode)
                    .Handle<HttpRequestException>()
                    .Handle<TaskCanceledException>()
                    .Handle<TimeoutRejectedException>(),
                    OnRetry = args =>
                    {
                        ILogger logger = context.ServiceProvider.GetRequiredService<ILogger<ClientService>>();
                        HttpRequestMessage request = args.Context.GetRequestMessage()!;

                        request.Options.TryGetValue(HttpOptionKeys.ClientName, out string? clientName);
                        request.Options.TryGetValue(HttpOptionKeys.RequestBody, out string? requestBody);
                        request.Options.TryGetValue(HttpOptionKeys.ResponseBody, out string? responseBody);
                        request.Options.TryGetValue(HttpOptionKeys.StatusCode, out int statusCode);
                        request.Options.TryGetValue(HttpOptionKeys.RequestUri, out Uri? requestUri);

                        logger.LogWarning(args.Outcome.Exception,
                            "{clientName} Retry #{attemptNumber} | {statusCode} {method} {uri} | Request body: {requestBody} | Response body: {responseBody}", [
                                clientName ?? "Standard",
                                args.AttemptNumber,
                                statusCode,
                                request.Method,
                                request.Headers.Host + requestUri?.PathAndQuery,
                                string.IsNullOrWhiteSpace(requestBody) ? "N/A" : requestBody,
                                string.IsNullOrWhiteSpace(responseBody) ? "N/A" : responseBody
                            ]);
                        return ValueTask.CompletedTask;
                    }
                });

                builder.AddTimeout(new TimeoutStrategyOptions
                {
                    Timeout = TimeSpan.FromSeconds(10)
                });
            });
            standardHttpClientBuilder.AddHttpMessageHandler<ValidationHandler>();

            //Adding proxy clients
            for (int i = 0; i < proxyList.Count; i++)
            {
                int buffer = i;

                IHttpClientBuilder proxyHttpClientBuilder = builder.Services.AddHttpClient(buffer.ToString(), client =>
                {
                    client.DefaultRequestVersion = HttpVersion.Version30;
                    client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
                    client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
                    client.DefaultRequestHeaders.Connection.Add("keep-alive");
                    client.Timeout = Timeout.InfiniteTimeSpan;
                }).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
                {
                    UseProxy = true,
                    Proxy = proxyList[buffer],
                    AutomaticDecompression = DecompressionMethods.All
                });
                proxyHttpClientBuilder.AddHttpMessageHandler<RateLimitHandler>();
                proxyHttpClientBuilder.AddResilienceHandler("pipeline", (builder, context) =>
                {
                    builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                    {
                        MaxRetryAttempts = 3,
                        Delay = TimeSpan.FromMilliseconds(300),
                        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .HandleResult(r => !r.IsSuccessStatusCode)
                        .Handle<HttpRequestException>()
                        .Handle<TaskCanceledException>()
                        .Handle<TimeoutRejectedException>(),
                        OnRetry = args =>
                        {
                            ILogger logger = context.ServiceProvider.GetRequiredService<ILogger<ClientService>>();
                            HttpRequestMessage request = args.Context.GetRequestMessage()!;

                            request.Options.TryGetValue(HttpOptionKeys.ClientName, out string? clientName);
                            request.Options.TryGetValue(HttpOptionKeys.RequestBody, out string? requestBody);
                            request.Options.TryGetValue(HttpOptionKeys.ResponseBody, out string? responseBody);
                            request.Options.TryGetValue(HttpOptionKeys.StatusCode, out int statusCode);
                            request.Options.TryGetValue(HttpOptionKeys.RequestUri, out Uri? requestUri);

                            logger.LogWarning(args.Outcome.Exception,
                                "{clientName} Retry #{attemptNumber} | {statusCode} {method} {uri} | Request body: {requestBody} | Response body: {responseBody}", [
                                clientName ?? "Standard",
                                args.AttemptNumber,
                                statusCode,
                                request.Method,
                                request.Headers.Host + requestUri?.PathAndQuery,
                                string.IsNullOrWhiteSpace(requestBody) ? "N/A" : requestBody,
                                string.IsNullOrWhiteSpace(responseBody) ? "N/A" : responseBody
                                ]);
                            return ValueTask.CompletedTask;
                        }
                    });

                    builder.AddTimeout(new TimeoutStrategyOptions
                    {
                        Timeout = TimeSpan.FromSeconds(10)
                    });
                });
                proxyHttpClientBuilder.AddHttpMessageHandler<ValidationHandler>();
            }

            return builder;
        }

        public static string RotatingProxyClient(string key, int? minId = null, int? maxId = null)
        {
            return _rotatingProxyIndexes.AddOrUpdate(key, _ => minId ?? 0, (_, current) => (current + 1) % (maxId ?? proxyCount)).ToString();
        }

        public static T Deserialize<T>(HttpRequestMessage request)
        {
            if (request.Options.TryGetValue(new HttpRequestOptionsKey<string>("ResponseBody"), out string? responseBody) && responseBody is not null)
            {
                T? deserializedObject;

                try
                {
                    deserializedObject = JsonSerializer.Deserialize<T>(responseBody, Helper.deserializeOptions);
                }

                catch (Exception ex)
                {
                    throw new HttpRequestException("Deserialization error", ex);
                }

                if (deserializedObject is null)
                {
                    throw new HttpRequestException("Deserialization object is null");
                }

                return deserializedObject;
            }

            else
            {
                throw new HttpRequestException("Response body is null");
            }
        }
    }
}
