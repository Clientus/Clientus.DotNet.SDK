using Clientus.ApiClient.Http;

namespace Clientus.ApiClient.Catalog;

/// <summary>Provides authenticated catalog operations controlled by Supabase RLS.</summary>
/// <remarks>
/// Creation is unavailable because the verified server workflow resolves or creates the caller's
/// company and supplies protected tenant and creator fields. Category mutations and inventory
/// operations are also not exposed.
/// </remarks>
public sealed class CatalogService
{
    private const string ItemFields =
        "id,company_id,created_by,kind,name,description,category,category_id,unit,price,photo_url," +
        "price_tax_mode,vat_rate,created_at,updated_at";
    private const string CategoryFields =
        "id,company_id,name,type,icon,color,sort_order,is_default,created_at,updated_at";
    private readonly ClientusHttpClient _http;

    /// <summary>Initializes the service with the shared authenticated transport.</summary>
    public CatalogService(ClientusHttpClient http)
    {
        ArgumentNullException.ThrowIfNull(http);
        _http = http;
    }

    /// <summary>Gets an exact RLS-visible catalog item.</summary>
    public async Task<CatalogItem?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        var rows = await _http.GetAsync<List<CatalogItem>>(
            $"/rest/v1/catalog_items?select={ItemFields}&{ExactId(id)}&limit=1", cancellationToken);
        return rows?.FirstOrDefault();
    }

    /// <summary>Lists all RLS-visible items ordered by kind and name ascending.</summary>
    public async Task<IReadOnlyList<CatalogItem>> ListAsync(CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        var rows = await _http.GetAsync<List<CatalogItem>>(
            $"/rest/v1/catalog_items?select={ItemFields}&order=kind.asc,name.asc", cancellationToken);
        return PostgRestQuery.OrEmpty(rows);
    }

    /// <summary>Lists RLS-visible items of one kind ordered by name ascending.</summary>
    public async Task<IReadOnlyList<CatalogItem>> GetByTypeAsync(
        CatalogItemType type,
        CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        ValidateEnum(type, nameof(type));
        var rows = await _http.GetAsync<List<CatalogItem>>(
            $"/rest/v1/catalog_items?select={ItemFields}&kind=eq.{EnumValue(type)}&order=name.asc",
            cancellationToken);
        return PostgRestQuery.OrEmpty(rows);
    }

    /// <summary>
    /// Searches name, description, and legacy category text using the verified case-insensitive
    /// contains query and optional item-kind filter.
    /// </summary>
    public async Task<IReadOnlyList<CatalogItem>> SearchAsync(
        string text,
        int limit = 20,
        CatalogItemType? type = null,
        CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        if (string.IsNullOrWhiteSpace(text)) return Array.Empty<CatalogItem>();
        if (limit is < 1 or > 50) throw new ArgumentOutOfRangeException(nameof(limit));
        if (type is not null) ValidateEnum(type.Value, nameof(type));

        var escaped = text.Trim().Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
        var pattern = Uri.EscapeDataString($"%{escaped}%");
        var typeFilter = type is null ? string.Empty : $"&kind=eq.{EnumValue(type.Value)}";
        var rows = await _http.GetAsync<List<CatalogItem>>(
            $"/rest/v1/catalog_items?select={ItemFields}{typeFilter}" +
            $"&or=(name.ilike.{pattern},description.ilike.{pattern},category.ilike.{pattern})" +
            $"&order=name.asc&limit={limit}", cancellationToken);
        return PostgRestQuery.OrEmpty(rows);
    }

    /// <summary>Lists RLS-visible categories by sort order and name ascending.</summary>
    public async Task<IReadOnlyList<CatalogCategory>> ListCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        var rows = await _http.GetAsync<List<CatalogCategory>>(
            $"/rest/v1/catalog_categories?select={CategoryFields}&order=sort_order.asc,name.asc",
            cancellationToken);
        return PostgRestQuery.OrEmpty(rows);
    }

    /// <summary>Determines whether an exact item identifier is visible through RLS.</summary>
    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        var rows = await _http.GetAsync<List<CatalogIdentity>>(
            $"/rest/v1/catalog_items?select=id&{ExactId(id)}&limit=1", cancellationToken);
        return rows?.Count > 0;
    }

    /// <summary>Gets the exact RLS-visible item count using a retryable HEAD request.</summary>
    public Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        return _http.HeadCountAsync("/rest/v1/catalog_items?select=id", cancellationToken);
    }

    /// <summary>
    /// Updates only verified writable fields. Identifier, company, creator, and timestamps are not
    /// written. The operation is authorized by RLS and PATCH is not retried.
    /// </summary>
    public async Task<CatalogItem> UpdateAsync(
        CatalogItem item,
        CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(item);
        PostgRestQuery.ValidateIdentifier(item.Id, nameof(item));
        ValidateEnum(item.Kind, nameof(item.Kind));
        ValidateEnum(item.PriceTaxMode, nameof(item.PriceTaxMode));
        if (string.IsNullOrWhiteSpace(item.Name))
            throw new ArgumentException("A catalog item name is required.", nameof(item));

        var rows = await _http.PatchAsync<List<CatalogItem>>(
            $"/rest/v1/catalog_items?select={ItemFields}&{ExactId(item.Id)}",
            new
            {
                kind = item.Kind,
                name = item.Name,
                description = item.Description,
                category = item.Category,
                category_id = item.CategoryId,
                unit = item.Unit,
                price = item.Price,
                photo_url = item.PhotoUrl,
                price_tax_mode = item.PriceTaxMode,
                vat_rate = item.VatRate
            }, cancellationToken);
        return rows?.SingleOrDefault()
            ?? throw new InvalidOperationException("The API did not return the updated catalog item.");
    }

    /// <summary>
    /// Deletes an exact RLS-visible item. Verified references from quotes, invoices, appointments,
    /// and work-report travel settings become null. Transient DELETE failures may retry.
    /// </summary>
    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        return _http.DeleteAsync($"/rest/v1/catalog_items?{ExactId(id)}", cancellationToken);
    }

    internal void ThrowIfDisposed() => _http.ThrowIfDisposed();

    private static string ExactId(string id) => PostgRestQuery.ExactFilter("id", id, nameof(id));
    private static string EnumValue<TEnum>(TEnum value) where TEnum : struct, Enum =>
        value.ToString().ToLowerInvariant();
    private static void ValidateEnum<TEnum>(TEnum value, string parameterName) where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value)) throw new ArgumentOutOfRangeException(parameterName);
    }

    private sealed class CatalogIdentity { public string Id { get; set; } = string.Empty; }
}
