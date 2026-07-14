using System.Text.Json;
using System.Text.Json.Serialization;

namespace Clientus.ApiClient.Quotes;

/// <summary>Represents a verified quote workflow status.</summary>
[JsonConverter(typeof(QuoteEnumJsonConverter<QuoteStatus>))]
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

internal sealed class QuoteEnumJsonConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String ||
            !Enum.TryParse<TEnum>(reader.GetString(), true, out var value))
        {
            throw new JsonException($"Invalid {typeof(TEnum).Name} value.");
        }

        return value;
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString().ToLowerInvariant());
}
