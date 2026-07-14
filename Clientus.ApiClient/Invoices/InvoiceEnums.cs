using System.Text.Json.Serialization;
using Clientus.ApiClient.Serialization;

namespace Clientus.ApiClient.Invoices;

/// <summary>Represents a verified invoice status stored by the backend.</summary>
[JsonConverter(typeof(LowercaseEnumJsonConverter<InvoiceStatus>))]
public enum InvoiceStatus
{
    /// <summary>The invoice is a draft.</summary>
    Draft,
    /// <summary>The invoice has been sent.</summary>
    Sent,
    /// <summary>The invoice has been partially paid.</summary>
    Partial,
    /// <summary>The invoice has been fully paid.</summary>
    Paid,
    /// <summary>The invoice is overdue.</summary>
    Overdue,
    /// <summary>The invoice has been cancelled.</summary>
    Cancelled
}

/// <summary>Represents the verified purpose of an invoice.</summary>
[JsonConverter(typeof(LowercaseEnumJsonConverter<InvoiceKind>))]
public enum InvoiceKind
{
    /// <summary>A standard invoice.</summary>
    Standard,
    /// <summary>A deposit invoice.</summary>
    Deposit,
    /// <summary>A final invoice.</summary>
    Final
}

/// <summary>Represents the verified invoice discount calculation kind.</summary>
[JsonConverter(typeof(LowercaseEnumJsonConverter<InvoiceDiscountKind>))]
public enum InvoiceDiscountKind
{
    /// <summary>No discount.</summary>
    None,
    /// <summary>A percentage discount.</summary>
    Percent,
    /// <summary>A fixed-value discount.</summary>
    Fixed
}

/// <summary>Represents the verified invoice discount scope.</summary>
[JsonConverter(typeof(LowercaseEnumJsonConverter<InvoiceDiscountScope>))]
public enum InvoiceDiscountScope
{
    /// <summary>The complete invoice.</summary>
    Total,
    /// <summary>Product lines.</summary>
    Products,
    /// <summary>Service lines.</summary>
    Services,
    /// <summary>Labor lines.</summary>
    Labor,
    /// <summary>Travel lines.</summary>
    Travel
}

/// <summary>Represents a verified invoice line category.</summary>
[JsonConverter(typeof(LowercaseEnumJsonConverter<InvoiceLineKind>))]
public enum InvoiceLineKind
{
    /// <summary>A product line.</summary>
    Product,
    /// <summary>A service line.</summary>
    Service,
    /// <summary>A labor line.</summary>
    Labor,
    /// <summary>A travel line.</summary>
    Travel,
    /// <summary>Another line category.</summary>
    Other
}

/// <summary>Represents a database-constrained installment cadence.</summary>
[JsonConverter(typeof(LowercaseEnumJsonConverter<InvoiceInstallmentCadence>))]
public enum InvoiceInstallmentCadence
{
    /// <summary>A monthly cadence.</summary>
    Monthly,
    /// <summary>A quarterly cadence.</summary>
    Quarterly
}
