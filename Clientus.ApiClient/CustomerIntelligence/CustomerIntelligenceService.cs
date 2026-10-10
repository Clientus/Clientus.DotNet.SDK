using Clientus.ApiClient.Http;
using Clientus.ApiClient.SdkNext;

namespace Clientus.ApiClient.CustomerIntelligence;

/// <summary>Read-only Customer Intelligence operations exposed by Clientus Public API v1 beta.5.</summary>
public sealed class CustomerIntelligenceService
{
    private readonly IClientusApiTransport _http;
    public CustomerIntelligenceService(ClientusHttpClient http) : this((IClientusApiTransport)http) { }
    internal CustomerIntelligenceService(IClientusApiTransport http) { ArgumentNullException.ThrowIfNull(http); _http = http; }

    private async Task<IReadOnlyList<SdkNextRecord>> ListAsync(string route, int page, int pageSize, CancellationToken token)
    {
        _http.ThrowIfDisposed();
        return await _http.GetAsync<List<SdkNextRecord>>(SdkNextRead.PageEndpoint(route, page, pageSize), token) ?? [];
    }

    public Task<IReadOnlyList<SdkNextRecord>> ListProfilesAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default) =>
        ListAsync("/api/v1/customer-intelligence/profiles", page, pageSize, cancellationToken);
    public Task<IReadOnlyList<SdkNextRecord>> ListSegmentsAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default) =>
        ListAsync("/api/v1/customer-intelligence/segments", page, pageSize, cancellationToken);
    public Task<IReadOnlyList<SdkNextRecord>> ListSegmentPerformanceAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default) =>
        ListAsync("/api/v1/customer-intelligence/segment-performance", page, pageSize, cancellationToken);
    public Task<IReadOnlyList<SdkNextRecord>> ListCampaignOutcomesAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default) =>
        ListAsync("/api/v1/customer-intelligence/campaign-outcomes", page, pageSize, cancellationToken);
}
