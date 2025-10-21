using System.Net;

namespace CryptoOculus
{
    public static class HttpRequestMessageExtensions
    {
        public static HttpRequestMessage WithVersion(this HttpRequestMessage request, string? version = null, HttpVersionPolicy policy = HttpVersionPolicy.RequestVersionOrLower)
        {
            request.Version = !String.IsNullOrWhiteSpace(version) ? new Version(version) : HttpVersion.Version30;
            request.VersionPolicy = policy;
            return request;
        }
    }
}
