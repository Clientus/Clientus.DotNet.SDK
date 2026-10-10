using Clientus.ApiClient.Http;
using Clientus.ApiClient.SdkNext;

namespace Clientus.ApiClient.RetailDistribution;

/// <summary>Read-only Retail Distribution operations exposed by Clientus Public API v1 beta.5.</summary>
public sealed class RetailDistributionService
{
    private readonly IClientusApiTransport _http;
    public RetailDistributionService(ClientusHttpClient http) : this((IClientusApiTransport)http) { }
    internal RetailDistributionService(IClientusApiTransport http) { ArgumentNullException.ThrowIfNull(http); _http = http; }

    private async Task<IReadOnlyList<SdkNextRecord>> ListAsync(string route, int page, int pageSize, CancellationToken token)
    {
        _http.ThrowIfDisposed();
        return await _http.GetAsync<List<SdkNextRecord>>(SdkNextRead.PageEndpoint(route, page, pageSize), token) ?? [];
    }

    public Task<IReadOnlyList<SdkNextRecord>> ListChannelsAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default) =>
        ListAsync("/api/v1/retail-distribution/channels", page, pageSize, cancellationToken);
    public Task<IReadOnlyList<SdkNextRecord>> ListSyncStatusAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default) =>
        ListAsync("/api/v1/retail-distribution/sync-status", page, pageSize, cancellationToken);
    public Task<IReadOnlyList<SdkNextRecord>> ListInboundEventsAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default) =>
        ListAsync("/api/v1/retail-distribution/inbound-events", page, pageSize, cancellationToken);
}
