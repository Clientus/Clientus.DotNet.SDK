using Clientus.ApiClient.Http;
using Clientus.ApiClient.SdkNext;

namespace Clientus.ApiClient.RetailIntelligence;

/// <summary>Read-only Advanced Retail Intelligence operations exposed by Clientus Public API v1 beta.5.</summary>
public sealed class RetailIntelligenceService
{
    private readonly IClientusApiTransport _http;
    public RetailIntelligenceService(ClientusHttpClient http) : this((IClientusApiTransport)http) { }
    internal RetailIntelligenceService(IClientusApiTransport http) { ArgumentNullException.ThrowIfNull(http); _http = http; }

    private async Task<IReadOnlyList<SdkNextRecord>> ListAsync(string route, int page, int pageSize, CancellationToken token)
    {
        _http.ThrowIfDisposed();
        return await _http.GetAsync<List<SdkNextRecord>>(SdkNextRead.PageEndpoint(route, page, pageSize), token) ?? [];
    }

    public Task<IReadOnlyList<SdkNextRecord>> ListProductPerformanceAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default) =>
        ListAsync("/api/v1/retail-intelligence/product-performance", page, pageSize, cancellationToken);
    public Task<IReadOnlyList<SdkNextRecord>> ListBasketAffinityAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default) =>
        ListAsync("/api/v1/retail-intelligence/basket-affinity", page, pageSize, cancellationToken);
    public Task<IReadOnlyList<SdkNextRecord>> ListLocationPerformanceAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default) =>
        ListAsync("/api/v1/retail-intelligence/location-performance", page, pageSize, cancellationToken);
    public Task<IReadOnlyList<SdkNextRecord>> ListStockHealthAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default) =>
        ListAsync("/api/v1/retail-intelligence/stock-health", page, pageSize, cancellationToken);
    public Task<IReadOnlyList<SdkNextRecord>> ListSalesTrendsAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default) =>
        ListAsync("/api/v1/retail-intelligence/sales-trends", page, pageSize, cancellationToken);
    public Task<IReadOnlyList<SdkNextRecord>> ListReturnReasonsAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default) =>
        ListAsync("/api/v1/retail-intelligence/return-reasons", page, pageSize, cancellationToken);
    public Task<IReadOnlyList<SdkNextRecord>> ListActionSignalsAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default) =>
        ListAsync("/api/v1/retail-intelligence/action-signals", page, pageSize, cancellationToken);
}
