using Clientus.ApiClient.Http;

namespace Clientus.ApiClient.Catalog;

/// <summary>Read-only catalog operations exposed by Clientus Public API v1.</summary>
public sealed class CatalogService
{
    private readonly IClientusApiTransport _http;
    public CatalogService(ClientusHttpClient http) : this((IClientusApiTransport)http) { }
    internal CatalogService(IClientusApiTransport http)
    {
        ArgumentNullException.ThrowIfNull(http);
        _http = http;
    }

    public Task<CatalogItem?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        ValidateId(id);
        return _http.GetAsync<CatalogItem>($"/api/v1/catalog/{Uri.EscapeDataString(id)}", cancellationToken);
    }

    public async Task<IReadOnlyList<CatalogItem>> ListAsync(CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        return await _http.GetAsync<List<CatalogItem>>("/api/v1/catalog", cancellationToken) ?? [];
    }

    [Obsolete("Catalog type filtering is not yet part of the verified Public API v1 SDK contract.")]
    public Task<IReadOnlyList<CatalogItem>> GetByTypeAsync(CatalogItemType type, CancellationToken cancellationToken = default) =>
        UnsupportedList(cancellationToken, "Catalog type filtering");

    [Obsolete("Catalog search is not yet part of the verified Public API v1 SDK contract.")]
    public Task<IReadOnlyList<CatalogItem>> SearchAsync(string text, int limit = 20, CatalogItemType? type = null, CancellationToken cancellationToken = default) =>
        UnsupportedList(cancellationToken, "Catalog search");

    [Obsolete("Catalog category listing is not yet exposed by the Public API v1 contract.")]
    public Task<IReadOnlyList<CatalogCategory>> ListCategoriesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        throw new NotSupportedException("Catalog category listing is not available in Clientus Public API v1.");
    }

    [Obsolete("Catalog existence probes are not yet exposed by Public API v1.")]
    public Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default) => Unsupported<bool>(cancellationToken, "Catalog existence");
    [Obsolete("Catalog exact counts are not yet exposed by Public API v1.")]
    public Task<long> CountAsync(CancellationToken cancellationToken = default) => Unsupported<long>(cancellationToken, "Catalog count");
    [Obsolete("Catalog mutation is not yet exposed by Public API v1.")]
    public Task<CatalogItem> UpdateAsync(CatalogItem item, CancellationToken cancellationToken = default) => Unsupported<CatalogItem>(cancellationToken, "Catalog update");
    [Obsolete("Catalog deletion is not yet exposed by Public API v1.")]
    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        throw new NotSupportedException("Catalog deletion is not available in Clientus Public API v1.");
    }

    internal void ThrowIfDisposed() => _http.ThrowIfDisposed();
    private static void ValidateId(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A catalog identifier is required.", nameof(id));
    }
    private static Task<T> Unsupported<T>(CancellationToken token, string operation)
    {
        token.ThrowIfCancellationRequested();
        throw new NotSupportedException($"{operation} is not available in Clientus Public API v1.");
    }
    private static Task<IReadOnlyList<CatalogItem>> UnsupportedList(CancellationToken token, string operation)
    {
        token.ThrowIfCancellationRequested();
        throw new NotSupportedException($"{operation} is not available in Clientus Public API v1.");
    }
}