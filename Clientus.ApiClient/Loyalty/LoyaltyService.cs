using Clientus.ApiClient.Http;
using Clientus.ApiClient.SdkNext;

namespace Clientus.ApiClient.Loyalty;

/// <summary>Read-only Loyalty operations exposed by Clientus Public API v1 beta.5.</summary>
public sealed class LoyaltyService
{
    private readonly IClientusApiTransport _http;
    public LoyaltyService(ClientusHttpClient http) : this((IClientusApiTransport)http) { }
    internal LoyaltyService(IClientusApiTransport http) { ArgumentNullException.ThrowIfNull(http); _http = http; }

    private async Task<IReadOnlyList<SdkNextRecord>> ListAsync(string route, int page, int pageSize, CancellationToken token)
    {
        _http.ThrowIfDisposed();
        return await _http.GetAsync<List<SdkNextRecord>>(SdkNextRead.PageEndpoint(route, page, pageSize), token) ?? [];
    }

    public Task<IReadOnlyList<SdkNextRecord>> ListProgramsAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default) =>
        ListAsync("/api/v1/loyalty/programs", page, pageSize, cancellationToken);
    public Task<IReadOnlyList<SdkNextRecord>> ListMovementsAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default) =>
        ListAsync("/api/v1/loyalty/movements", page, pageSize, cancellationToken);
    public Task<IReadOnlyList<SdkNextRecord>> ListRewardsAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default) =>
        ListAsync("/api/v1/loyalty/rewards", page, pageSize, cancellationToken);
    public Task<IReadOnlyList<SdkNextRecord>> ListCampaignsAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default) =>
        ListAsync("/api/v1/loyalty/campaigns", page, pageSize, cancellationToken);
    public Task<IReadOnlyList<SdkNextRecord>> ListEarnRulesAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default) =>
        ListAsync("/api/v1/loyalty/earn-rules", page, pageSize, cancellationToken);

    public Task<SdkNextRecord?> GetMemberAsync(string id, CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        return _http.GetAsync<SdkNextRecord>($"/api/v1/loyalty/members/{SdkNextRead.EscapedId(id)}", cancellationToken);
    }

    public Task<SdkNextRecord?> GetBalanceAsync(string id, CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        return _http.GetAsync<SdkNextRecord>($"/api/v1/loyalty/balances/{SdkNextRead.EscapedId(id)}", cancellationToken);
    }
}
