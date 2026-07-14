using System.Text.Json.Serialization;

namespace Clientus.ApiClient.Quotes;

/// <summary>Identifies the verified quote discount calculation kind.</summary>
[JsonConverter(typeof(QuoteEnumJsonConverter<QuoteDiscountKind>))]
public enum QuoteDiscountKind
{
    /// <summary>No discount.</summary>
    None,
    /// <summary>A percentage discount.</summary>
    Percent,
    /// <summary>A fixed-value discount.</summary>
    Fixed
}
