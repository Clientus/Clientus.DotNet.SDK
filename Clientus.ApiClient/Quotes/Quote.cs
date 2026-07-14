using System.Text.Json;
using System.Text.Json.Serialization;

namespace Clientus.ApiClient.Quotes;

/// <summary>Represents a quote visible to the authenticated caller under row-level security.</summary>
public sealed class Quote
{
    /// <summary>Gets or sets the quote identifier.</summary>
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    /// <summary>Gets or sets the owning company identifier.</summary>
    [JsonPropertyName("company_id")] public string CompanyId { get; set; } = string.Empty;
    /// <summary>Gets or sets the linked client identifier.</summary>
    [JsonPropertyName("client_id")] public string? ClientId { get; set; }
    /// <summary>Gets or sets the creating user identifier.</summary>
    [JsonPropertyName("created_by")] public string CreatedBy { get; set; } = string.Empty;
    /// <summary>Gets or sets the company-scoped quote number.</summary>
    [JsonPropertyName("number")] public string Number { get; set; } = string.Empty;
    /// <summary>Gets or sets the title.</summary>
    [JsonPropertyName("title")] public string? Title { get; set; }
    /// <summary>Gets or sets the notes.</summary>
    [JsonPropertyName("notes")] public string? Notes { get; set; }
    /// <summary>Gets or sets the workflow status.</summary>
    [JsonPropertyName("status")] public QuoteStatus Status { get; set; }
    /// <summary>Gets or sets the currency code.</summary>
    [JsonPropertyName("currency")] public string Currency { get; set; } = string.Empty;
    /// <summary>Gets or sets the net subtotal.</summary>
    [JsonPropertyName("subtotal")] public decimal Subtotal { get; set; }
    /// <summary>Gets or sets the tax rate.</summary>
    [JsonPropertyName("tax_rate")] public decimal TaxRate { get; set; }
    /// <summary>Gets or sets the tax amount.</summary>
    [JsonPropertyName("tax_amount")] public decimal TaxAmount { get; set; }
    /// <summary>Gets or sets the total.</summary>
    [JsonPropertyName("total")] public decimal Total { get; set; }
    /// <summary>Gets or sets the database date representation for quote validity.</summary>
    [JsonPropertyName("valid_until")] public string? ValidUntil { get; set; }
    /// <summary>Gets or sets the database date representation for issuance.</summary>
    [JsonPropertyName("issued_at")] public string IssuedAt { get; set; } = string.Empty;
    /// <summary>Gets or sets when the quote was first sent.</summary>
    [JsonPropertyName("sent_at")] public DateTimeOffset? SentAt { get; set; }
    /// <summary>Gets or sets when the quote was accepted.</summary>
    [JsonPropertyName("accepted_at")] public DateTimeOffset? AcceptedAt { get; set; }
    /// <summary>Gets or sets when the quote was rejected.</summary>
    [JsonPropertyName("rejected_at")] public DateTimeOffset? RejectedAt { get; set; }
    /// <summary>Gets or sets the client data snapshot stored with the quote.</summary>
    [JsonPropertyName("client_snapshot")] public JsonElement? ClientSnapshot { get; set; }
    /// <summary>Gets or sets the public token. Public-token operations are not exposed by this service.</summary>
    [JsonPropertyName("public_token")] public string PublicToken { get; set; } = string.Empty;
    /// <summary>Gets or sets the client's response note.</summary>
    [JsonPropertyName("client_response_note")] public string? ClientResponseNote { get; set; }
    /// <summary>Gets or sets the discount kind.</summary>
    [JsonPropertyName("discount_kind")] public QuoteDiscountKind? DiscountKind { get; set; }
    /// <summary>Gets or sets the configured discount value.</summary>
    [JsonPropertyName("discount_value")] public decimal? DiscountValue { get; set; }
    /// <summary>Gets or sets the discount scope.</summary>
    [JsonPropertyName("discount_scope")] public QuoteDiscountScope? DiscountScope { get; set; }
    /// <summary>Gets or sets the calculated discount amount.</summary>
    [JsonPropertyName("discount_amount")] public decimal? DiscountAmount { get; set; }
    /// <summary>Gets or sets when the quote was created.</summary>
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }
    /// <summary>Gets or sets when the quote was last updated.</summary>
    [JsonPropertyName("updated_at")] public DateTimeOffset UpdatedAt { get; set; }
}
