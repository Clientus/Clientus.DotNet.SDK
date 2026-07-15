using System.Text.Json.Serialization;

namespace Clientus.ApiClient.Catalog;

/// <summary>Represents an RLS-visible catalog category.</summary>
public sealed class CatalogCategory
{
    /// <summary>Gets or sets the category identifier.</summary>
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    /// <summary>Gets or sets the owning company identifier.</summary>
    [JsonPropertyName("company_id")] public string CompanyId { get; set; } = string.Empty;
    /// <summary>Gets or sets the category name.</summary>
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the supported item type.</summary>
    [JsonPropertyName("type")] public CatalogCategoryType Type { get; set; }
    /// <summary>Gets or sets the optional icon value.</summary>
    [JsonPropertyName("icon")] public string? Icon { get; set; }
    /// <summary>Gets or sets the optional color value.</summary>
    [JsonPropertyName("color")] public string? Color { get; set; }
    /// <summary>Gets or sets the category sort order.</summary>
    [JsonPropertyName("sort_order")] public int SortOrder { get; set; }
    /// <summary>Gets or sets whether the category is a protected default.</summary>
    [JsonPropertyName("is_default")] public bool IsDefault { get; set; }
    /// <summary>Gets or sets when the category was created.</summary>
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }
    /// <summary>Gets or sets when the category was last updated.</summary>
    [JsonPropertyName("updated_at")] public DateTimeOffset UpdatedAt { get; set; }
}
