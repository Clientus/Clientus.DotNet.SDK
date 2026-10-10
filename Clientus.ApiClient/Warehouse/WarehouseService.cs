using Clientus.ApiClient.Http;
using Clientus.ApiClient.SdkNext;

namespace Clientus.ApiClient.Warehouse;

/// <summary>Read-only Warehouse operations exposed by Clientus Public API v1 beta.5.</summary>
public sealed class WarehouseService
{
    private readonly IClientusApiTransport _http;
    public WarehouseService(ClientusHttpClient http) : this((IClientusApiTransport)http) { }
    internal WarehouseService(IClientusApiTransport http) { ArgumentNullException.ThrowIfNull(http); _http = http; }

    public async Task<IReadOnlyList<SdkNextRecord>> ListStockAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        return await _http.GetAsync<List<SdkNextRecord>>(SdkNextRead.PageEndpoint("/api/v1/warehouse/stock", page, pageSize), cancellationToken) ?? [];
    }
}
