using Clientus.ApiClient.Http;
using Clientus.ApiClient.SdkNext;

namespace Clientus.ApiClient.Website;

/// <summary>Read-only Website operations exposed by Clientus Public API v1 beta.5.</summary>
public sealed class WebsiteService
{
    private readonly IClientusApiTransport _http;
    public WebsiteService(ClientusHttpClient http) : this((IClientusApiTransport)http) { }
    internal WebsiteService(IClientusApiTransport http) { ArgumentNullException.ThrowIfNull(http); _http = http; }

    private async Task<IReadOnlyList<SdkNextRecord>> ListAsync(string route, int page, int pageSize, CancellationToken token)
    {
        _http.ThrowIfDisposed();
        return await _http.GetAsync<List<SdkNextRecord>>(SdkNextRead.PageEndpoint(route, page, pageSize), token) ?? [];
    }

    public Task<IReadOnlyList<SdkNextRecord>> ListSitesAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default) =>
        ListAsync("/api/v1/website/sites", page, pageSize, cancellationToken);
    public Task<IReadOnlyList<SdkNextRecord>> ListPagesAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default) =>
        ListAsync("/api/v1/website/pages", page, pageSize, cancellationToken);
}
