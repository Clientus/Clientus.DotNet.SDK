using Clientus.ApiClient.Http;
using Clientus.ApiClient.SdkNext;

namespace Clientus.ApiClient.Home;

/// <summary>
/// Read-only household-scoped Home operations exposed by Clientus Public API v1.
/// HOME.LONG.3B is sandbox/grant-only; live authorization remains fail-closed server-side.
/// </summary>
public sealed class HomeService
{
    private readonly IClientusApiTransport _http;

    public HomeService(ClientusHttpClient http) : this((IClientusApiTransport)http) { }

    internal HomeService(IClientusApiTransport http)
    {
        ArgumentNullException.ThrowIfNull(http);
        _http = http;
    }

    public Task<SdkNextRecord?> GetContextAsync(CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        return _http.GetAsync<SdkNextRecord>("/api/v1/home/context", cancellationToken);
    }

    public async Task<IReadOnlyList<SdkNextRecord>> ListMembersAsync(
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        return await _http.GetAsync<List<SdkNextRecord>>(
            SdkNextRead.PageEndpoint("/api/v1/home/members", page, pageSize),
            cancellationToken) ?? [];
    }

    public Task<SdkNextRecord?> GetDocumentsOverviewAsync(CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        return _http.GetAsync<SdkNextRecord>("/api/v1/home/documents/overview", cancellationToken);
    }
}
