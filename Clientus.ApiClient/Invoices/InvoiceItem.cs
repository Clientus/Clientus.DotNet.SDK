using System.Text.Json.Serialization;

namespace Clientus.ApiClient.Invoices;

/// <summary>Represents an invoice line and its historical pricing and VAT snapshots.</summary>
public sealed class InvoiceItem
{
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("invoice_id")] public string InvoiceId { get; set; } = string.Empty;
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("catalog_item_id")] public string? CatalogItemId { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("quantity")] public decimal Quantity { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("unit")] public string? Unit { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("unit_price")] public decimal UnitPrice { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("line_total")] public decimal LineTotal { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("position")] public int Position { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("line_kind")] public InvoiceLineKind? LineKind { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("price_tax_mode_snapshot")] public string? PriceTaxModeSnapshot { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("vat_rate_snapshot")] public decimal? VatRateSnapshot { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("discount_snapshot")] public decimal? DiscountSnapshot { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("unit_price_input")] public decimal? UnitPriceInput { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("unit_price_net")] public decimal? UnitPriceNet { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("unit_price_gross")] public decimal? UnitPriceGross { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("net_amount")] public decimal? NetAmount { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("vat_amount")] public decimal? VatAmount { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("gross_amount")] public decimal? GrossAmount { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }
}

