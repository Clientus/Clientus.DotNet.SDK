using System.Text.Json;
using System.Text.Json.Serialization;

namespace Clientus.ApiClient.Serialization;

internal sealed class LowercaseEnumJsonConverter<TEnum> : JsonConverter<TEnum>
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
