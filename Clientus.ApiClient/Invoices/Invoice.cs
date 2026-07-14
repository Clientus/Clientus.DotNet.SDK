using System.Text.Json;
using System.Text.Json.Serialization;

namespace Clientus.ApiClient.Invoices;

/// <summary>Represents an invoice visible to the authenticated caller under row-level security.</summary>
public sealed class Invoice
{
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("company_id")] public string CompanyId { get; set; } = string.Empty;
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("client_id")] public string? ClientId { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("quote_id")] public string? QuoteId { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("created_by")] public string CreatedBy { get; set; } = string.Empty;
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("number")] public string Number { get; set; } = string.Empty;
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("title")] public string? Title { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("notes")] public string? Notes { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("status")] public InvoiceStatus Status { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("currency")] public string Currency { get; set; } = string.Empty;
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("subtotal")] public decimal Subtotal { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("tax_rate")] public decimal TaxRate { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("tax_amount")] public decimal TaxAmount { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("total")] public decimal Total { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("issued_at")] public string IssuedAt { get; set; } = string.Empty;
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("due_at")] public string? DueAt { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("sent_at")] public DateTimeOffset? SentAt { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("paid_at")] public DateTimeOffset? PaidAt { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("client_snapshot")] public JsonElement? ClientSnapshot { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("public_token")] public string PublicToken { get; set; } = string.Empty;
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("kind")] public InvoiceKind Kind { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("parent_quote_id")] public string? ParentQuoteId { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("deposit_percentage")] public decimal? DepositPercentage { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("deposit_of_amount")] public decimal? DepositOfAmount { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("qr_reference")] public string? QrReference { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("paid_amount")] public decimal PaidAmount { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("remaining_amount")] public decimal? RemainingAmount { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("discount_kind")] public InvoiceDiscountKind? DiscountKind { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("discount_value")] public decimal? DiscountValue { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("discount_scope")] public InvoiceDiscountScope? DiscountScope { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("discount_amount")] public decimal? DiscountAmount { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("installment_plan_id")] public string? InstallmentPlanId { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("installment_number")] public short? InstallmentNumber { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("installment_total")] public short? InstallmentTotal { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("parent_invoice_id")] public string? ParentInvoiceId { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("installment_cadence")] public InvoiceInstallmentCadence? InstallmentCadence { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("installment_state")] public string? InstallmentState { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("is_installment_source")] public bool IsInstallmentSource { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("payment_method")] public string? PaymentMethod { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("payment_link_url")] public string? PaymentLinkUrl { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("stripe_checkout_session_id")] public string? StripeCheckoutSessionId { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("stripe_payment_intent_id")] public string? StripePaymentIntentId { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("payment_amount")] public decimal? PaymentAmount { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("payment_currency")] public string? PaymentCurrency { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("payment_terms_days")] public int? PaymentTermsDays { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("payment_account_id")] public string? PaymentAccountId { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("payment_account_snapshot")] public JsonElement? PaymentAccountSnapshot { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("pdf_storage_path")] public string? PdfStoragePath { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("reminders_sent")] public JsonElement RemindersSent { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("work_report_id")] public string? WorkReportId { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("subscription_id")] public string? SubscriptionId { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("subscription_cycle_id")] public string? SubscriptionCycleId { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }
    /// <summary>Gets or sets the corresponding verified invoice database field.</summary>
    [JsonPropertyName("updated_at")] public DateTimeOffset UpdatedAt { get; set; }
}

