using System.Text.Json.Serialization;
using Clientus.ApiClient.Serialization;

namespace Clientus.ApiClient.Catalog;

/// <summary>Identifies the database-constrained catalog item kind.</summary>
[JsonConverter(typeof(LowercaseEnumJsonConverter<CatalogItemType>))]
public enum CatalogItemType
{
    /// <summary>A physical or supplied product.</summary>
    Product,
    /// <summary>A provided service.</summary>
    Service
}

/// <summary>Controls whether an item inherits or overrides the company tax mode.</summary>
[JsonConverter(typeof(SnakeCaseLowerEnumJsonConverter<CatalogPriceTaxMode>))]
public enum CatalogPriceTaxMode
{
    /// <summary>Uses the company catalog tax mode.</summary>
    Inherit,
    /// <summary>The stored price excludes tax.</summary>
    TaxExcluded,
    /// <summary>The stored price includes tax.</summary>
    TaxIncluded,
    /// <summary>The item is not taxed.</summary>
    NoTax
}

/// <summary>Identifies which catalog item kinds a category supports.</summary>
[JsonConverter(typeof(LowercaseEnumJsonConverter<CatalogCategoryType>))]
public enum CatalogCategoryType
{
    /// <summary>The category supports products.</summary>
    Product,
    /// <summary>The category supports services.</summary>
    Service,
    /// <summary>The category supports both item kinds.</summary>
    Both
}
