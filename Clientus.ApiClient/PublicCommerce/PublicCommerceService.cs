using Clientus.ApiClient.Http;
using Clientus.ApiClient.SdkNext;

namespace Clientus.ApiClient.PublicCommerce;

/// <summary>Read-only Public Commerce operations exposed by Clientus Public API v1 beta.5.</summary>
public sealed class PublicCommerceService
{
    private readonly IClientusApiTransport _http;
    public PublicCommerceService(ClientusHttpClient http) : this((IClientusApiTransport)http) { }
    internal PublicCommerceService(IClientusApiTransport http) { ArgumentNullException.ThrowIfNull(http); _http = http; }

    public Task<SdkNextRecord?> GetSurfaceAsync(CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        return _http.GetAsync<SdkNextRecord>("/api/v1/public-commerce/surface", cancellationToken);
    }
}
