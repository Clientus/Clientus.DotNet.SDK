using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Clientus.ApiClient.Serialization;

internal sealed partial class SnakeCaseLowerEnumJsonConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException($"Invalid {typeof(TEnum).Name} value.");

        var normalized = reader.GetString()?.Replace("_", string.Empty, StringComparison.Ordinal);
        foreach (var value in Enum.GetValues<TEnum>())
        {
            if (string.Equals(value.ToString(), normalized, StringComparison.OrdinalIgnoreCase)) return value;
        }
        throw new JsonException($"Invalid {typeof(TEnum).Name} value.");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options) =>
        writer.WriteStringValue(WordBoundary().Replace(value.ToString(), "$1_$2").ToLowerInvariant());

    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex WordBoundary();
}
