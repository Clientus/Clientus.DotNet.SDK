using Clientus.ApiClient.Http;
using Clientus.ApiClient.SdkNext;

namespace Clientus.ApiClient.RetailReturns;

/// <summary>Read-only Retail Returns operations exposed by Clientus Public API v1 beta.5.</summary>
public sealed class RetailReturnsService
{
    private readonly IClientusApiTransport _http;
    public RetailReturnsService(ClientusHttpClient http) : this((IClientusApiTransport)http) { }
    internal RetailReturnsService(IClientusApiTransport http) { ArgumentNullException.ThrowIfNull(http); _http = http; }

    public async Task<IReadOnlyList<SdkNextRecord>> ListAsync(
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default,
        string? businessUnitId = null)
    {
        _http.ThrowIfDisposed();
        return await _http.GetAsync<List<SdkNextRecord>>(
            SdkNextRead.PageEndpoint("/api/v1/retail-returns", page, pageSize, businessUnitId),
            cancellationToken) ?? [];
    }

    public Task<SdkNextRecord?> GetAsync(
        string id,
        CancellationToken cancellationToken = default,
        string? businessUnitId = null)
    {
        _http.ThrowIfDisposed();
        return _http.GetAsync<SdkNextRecord>(
            SdkNextRead.DetailEndpoint("/api/v1/retail-returns", id, businessUnitId),
            cancellationToken);
    }
}
