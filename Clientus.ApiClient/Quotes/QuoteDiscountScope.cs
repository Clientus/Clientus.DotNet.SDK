using System.Text.Json.Serialization;
using Clientus.ApiClient.Serialization;

namespace Clientus.ApiClient.Quotes;

/// <summary>Identifies which verified quote line categories receive a discount.</summary>
[JsonConverter(typeof(LowercaseEnumJsonConverter<QuoteDiscountScope>))]
public enum QuoteDiscountScope
{
    /// <summary>All quote lines.</summary>
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
