using System.Text.Json.Serialization;

namespace Clientus.ApiClient.Quotes;

/// <summary>Represents a verified quote line and its historical pricing and VAT snapshots.</summary>
public sealed class QuoteItem
{
    /// <summary>Gets or sets the item identifier.</summary>
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    /// <summary>Gets or sets the parent quote identifier.</summary>
    [JsonPropertyName("quote_id")] public string QuoteId { get; set; } = string.Empty;
    /// <summary>Gets or sets the linked catalog item identifier.</summary>
    [JsonPropertyName("catalog_item_id")] public string? CatalogItemId { get; set; }
    /// <summary>Gets or sets the line description.</summary>
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
    /// <summary>Gets or sets the quantity.</summary>
    [JsonPropertyName("quantity")] public decimal Quantity { get; set; }
    /// <summary>Gets or sets the unit label.</summary>
    [JsonPropertyName("unit")] public string? Unit { get; set; }
    /// <summary>Gets or sets the stored unit price.</summary>
    [JsonPropertyName("unit_price")] public decimal UnitPrice { get; set; }
    /// <summary>Gets or sets the stored line total.</summary>
    [JsonPropertyName("line_total")] public decimal LineTotal { get; set; }
    /// <summary>Gets or sets the line position.</summary>
    [JsonPropertyName("position")] public int Position { get; set; }
    /// <summary>Gets or sets the line category.</summary>
    [JsonPropertyName("line_kind")] public QuoteLineKind? LineKind { get; set; }
    /// <summary>Gets or sets the historical price tax mode.</summary>
    [JsonPropertyName("price_tax_mode_snapshot")] public string? PriceTaxModeSnapshot { get; set; }
    /// <summary>Gets or sets the historical VAT rate.</summary>
    [JsonPropertyName("vat_rate_snapshot")] public decimal? VatRateSnapshot { get; set; }
    /// <summary>Gets or sets the historical line discount.</summary>
    [JsonPropertyName("discount_snapshot")] public decimal? DiscountSnapshot { get; set; }
    /// <summary>Gets or sets the entered unit price snapshot.</summary>
    [JsonPropertyName("unit_price_input")] public decimal? UnitPriceInput { get; set; }
    /// <summary>Gets or sets the net unit price snapshot.</summary>
    [JsonPropertyName("unit_price_net")] public decimal? UnitPriceNet { get; set; }
    /// <summary>Gets or sets the gross unit price snapshot.</summary>
    [JsonPropertyName("unit_price_gross")] public decimal? UnitPriceGross { get; set; }
    /// <summary>Gets or sets the net amount snapshot.</summary>
    [JsonPropertyName("net_amount")] public decimal? NetAmount { get; set; }
    /// <summary>Gets or sets the VAT amount snapshot.</summary>
    [JsonPropertyName("vat_amount")] public decimal? VatAmount { get; set; }
    /// <summary>Gets or sets the gross amount snapshot.</summary>
    [JsonPropertyName("gross_amount")] public decimal? GrossAmount { get; set; }
    /// <summary>Gets or sets when the item was created.</summary>
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }
}
