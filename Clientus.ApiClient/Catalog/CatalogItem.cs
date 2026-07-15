using System.Text.Json.Serialization;

namespace Clientus.ApiClient.Catalog;

/// <summary>Represents a catalog item visible to the authenticated caller through RLS.</summary>
public sealed class CatalogItem
{
    /// <summary>Gets or sets the item identifier.</summary>
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    /// <summary>Gets or sets the owning company identifier.</summary>
    [JsonPropertyName("company_id")] public string CompanyId { get; set; } = string.Empty;
    /// <summary>Gets or sets the creating user identifier.</summary>
    [JsonPropertyName("created_by")] public string CreatedBy { get; set; } = string.Empty;
    /// <summary>Gets or sets the product or service kind.</summary>
    [JsonPropertyName("kind")] public CatalogItemType Kind { get; set; }
    /// <summary>Gets or sets the item name.</summary>
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the optional description.</summary>
    [JsonPropertyName("description")] public string? Description { get; set; }
    /// <summary>Gets or sets the legacy category text.</summary>
    [JsonPropertyName("category")] public string? Category { get; set; }
    /// <summary>Gets or sets the linked category identifier.</summary>
    [JsonPropertyName("category_id")] public string? CategoryId { get; set; }
    /// <summary>Gets or sets the free-form unit label.</summary>
    [JsonPropertyName("unit")] public string? Unit { get; set; }
    /// <summary>Gets or sets the optional item price.</summary>
    [JsonPropertyName("price")] public decimal? Price { get; set; }
    /// <summary>Gets or sets the optional photo URL.</summary>
    [JsonPropertyName("photo_url")] public string? PhotoUrl { get; set; }
    /// <summary>Gets or sets the item price tax mode.</summary>
    [JsonPropertyName("price_tax_mode")] public CatalogPriceTaxMode PriceTaxMode { get; set; }
    /// <summary>Gets or sets the optional VAT rate.</summary>
    [JsonPropertyName("vat_rate")] public decimal? VatRate { get; set; }
    /// <summary>Gets or sets when the item was created.</summary>
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }
    /// <summary>Gets or sets when the item was last updated.</summary>
    [JsonPropertyName("updated_at")] public DateTimeOffset UpdatedAt { get; set; }
}
