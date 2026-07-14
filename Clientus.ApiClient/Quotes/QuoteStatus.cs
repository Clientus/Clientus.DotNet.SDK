using System.Text.Json.Serialization;
using Clientus.ApiClient.Serialization;

namespace Clientus.ApiClient.Quotes;

/// <summary>Represents a verified quote workflow status.</summary>
[JsonConverter(typeof(LowercaseEnumJsonConverter<QuoteStatus>))]
public enum QuoteStatus
{
    /// <summary>The quote is a draft.</summary>
    Draft,
    /// <summary>The quote has been sent.</summary>
    Sent,
    /// <summary>The quote has been accepted.</summary>
    Accepted,
    /// <summary>The quote has been rejected.</summary>
    Rejected,
    /// <summary>The quote has expired.</summary>
    Expired
}
