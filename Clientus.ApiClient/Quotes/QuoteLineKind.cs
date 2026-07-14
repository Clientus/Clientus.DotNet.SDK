using System.Text.Json.Serialization;

namespace Clientus.ApiClient.Quotes;

/// <summary>Identifies the verified category of a quote line.</summary>
[JsonConverter(typeof(QuoteEnumJsonConverter<QuoteLineKind>))]
public enum QuoteLineKind
{
    /// <summary>A product line.</summary>
    Product,
    /// <summary>A service line.</summary>
    Service,
    /// <summary>A labor line.</summary>
    Labor,
    /// <summary>A travel line.</summary>
    Travel,
    /// <summary>Another kind of line.</summary>
    Other
}
