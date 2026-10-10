using Clientus.ApiClient.Http;
using Clientus.ApiClient.SdkNext;

namespace Clientus.ApiClient.Publishing;

/// <summary>Read-only Publishing operations exposed by Clientus Public API v1 beta.5.</summary>
public sealed class PublishingService
{
    private readonly IClientusApiTransport _http;
    public PublishingService(ClientusHttpClient http) : this((IClientusApiTransport)http) { }
    internal PublishingService(IClientusApiTransport http) { ArgumentNullException.ThrowIfNull(http); _http = http; }

    private async Task<IReadOnlyList<SdkNextRecord>> ListAsync(string route, int page, int pageSize, CancellationToken token)
    {
        _http.ThrowIfDisposed();
        return await _http.GetAsync<List<SdkNextRecord>>(SdkNextRead.PageEndpoint(route, page, pageSize), token) ?? [];
    }

    public Task<IReadOnlyList<SdkNextRecord>> ListResourcesAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default) =>
        ListAsync("/api/v1/publishing/resources", page, pageSize, cancellationToken);
    public Task<IReadOnlyList<SdkNextRecord>> ListEntriesAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default) =>
        ListAsync("/api/v1/publishing/entries", page, pageSize, cancellationToken);
    public Task<IReadOnlyList<SdkNextRecord>> ListDeliveriesAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default) =>
        ListAsync("/api/v1/publishing/deliveries", page, pageSize, cancellationToken);
}
