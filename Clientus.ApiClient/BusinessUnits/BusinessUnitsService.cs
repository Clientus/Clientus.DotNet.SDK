using Clientus.ApiClient.Http;
using Clientus.ApiClient.SdkNext;

namespace Clientus.ApiClient.BusinessUnits;

/// <summary>Read-only Business Units operations exposed by Clientus Public API v1 beta.5.</summary>
public sealed class BusinessUnitsService
{
    private readonly IClientusApiTransport _http;
    public BusinessUnitsService(ClientusHttpClient http) : this((IClientusApiTransport)http) { }
    internal BusinessUnitsService(IClientusApiTransport http) { ArgumentNullException.ThrowIfNull(http); _http = http; }

    public async Task<IReadOnlyList<SdkNextRecord>> ListAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        return await _http.GetAsync<List<SdkNextRecord>>(SdkNextRead.PageEndpoint("/api/v1/business-units", page, pageSize), cancellationToken) ?? [];
    }

    public Task<SdkNextRecord?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        return _http.GetAsync<SdkNextRecord>($"/api/v1/business-units/{SdkNextRead.EscapedId(id)}", cancellationToken);
    }

    public Task<SdkNextRecord?> GetContextAsync(CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        return _http.GetAsync<SdkNextRecord>("/api/v1/business-units/context", cancellationToken);
    }
}
