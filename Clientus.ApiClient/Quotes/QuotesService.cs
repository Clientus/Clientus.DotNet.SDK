using Clientus.ApiClient.Http;

namespace Clientus.ApiClient.Quotes;

/// <summary>Read-only quote operations exposed by Clientus Public API v1.</summary>
public class QuotesService
{
    private readonly IClientusApiTransport _http;
    public QuotesService(ClientusHttpClient http) : this((IClientusApiTransport)http) { }
    internal QuotesService(IClientusApiTransport http)
    {
        ArgumentNullException.ThrowIfNull(http);
        _http = http;
    }

    public Task<Quote?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        ValidateId(id);
        return _http.GetAsync<Quote>($"/api/v1/quotes/{Uri.EscapeDataString(id)}", cancellationToken);
    }

    public async Task<IReadOnlyList<Quote>> ListAsync(CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        return await _http.GetAsync<List<Quote>>("/api/v1/quotes", cancellationToken) ?? [];
    }

    [Obsolete("Quote item expansion is not yet exposed by the Public API v1 contract.")]
    public Task<QuoteWithItems?> GetWithItemsAsync(string id, CancellationToken cancellationToken = default) =>
        Unsupported<QuoteWithItems?>(cancellationToken, "Quote item expansion");

    [Obsolete("Quote existence probes are not yet exposed by Public API v1.")]
    public Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default) =>
        Unsupported<bool>(cancellationToken, "Quote existence");

    [Obsolete("Quote exact counts are not yet exposed by Public API v1.")]
    public Task<long> CountAsync(CancellationToken cancellationToken = default) =>
        Unsupported<long>(cancellationToken, "Quote count");

    [Obsolete("Direct quote status mutation is disabled. No Public API v1 write workflow exists yet.")]
    public Task<Quote> UpdateStatusAsync(string id, QuoteStatus targetStatus, CancellationToken cancellationToken = default) =>
        Unsupported<Quote>(cancellationToken, "Quote status mutation");

    [Obsolete("Quote deletion is not yet exposed by Public API v1.")]
    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        throw new NotSupportedException("Quote deletion is not available in Clientus Public API v1.");
    }

    internal void ThrowIfDisposed() => _http.ThrowIfDisposed();
    private static void ValidateId(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A quote identifier is required.", nameof(id));
    }
    private static Task<T> Unsupported<T>(CancellationToken token, string operation)
    {
        token.ThrowIfCancellationRequested();
        throw new NotSupportedException($"{operation} is not available in Clientus Public API v1.");
    }
}