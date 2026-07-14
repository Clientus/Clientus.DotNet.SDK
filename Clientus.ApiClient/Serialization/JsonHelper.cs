using System.Text.Json;

namespace Clientus.ApiClient.Serialization;

/// <summary>
/// Provides shared JSON serialization and deserialization utilities
/// for the Clientus API client.
/// </summary>
public static class JsonHelper
{
    private static readonly JsonSerializerOptions Options =
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

    /// <summary>
    /// Serializes the specified value to a JSON string.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value to serialize.
    /// </typeparam>
    /// <param name="value">
    /// The value to serialize.
    /// </param>
    /// <returns>
    /// A JSON string representing the specified value.
    /// </returns>
    public static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, Options);
    }

    /// <summary>
    /// Deserializes the specified JSON string into an object.
    /// </summary>
    /// <typeparam name="T">
    /// The target object type.
    /// </typeparam>
    /// <param name="json">
    /// The JSON string to deserialize.
    /// </param>
    /// <returns>
    /// The deserialized object, or <see langword="null"/> when the JSON
    /// represents a null value.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="json"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="JsonException">
    /// Thrown when the JSON is invalid or cannot be converted to
    /// <typeparamref name="T"/>.
    /// </exception>
    public static T? Deserialize<T>(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return JsonSerializer.Deserialize<T>(json, Options);
    }

    /// <summary>
    /// Gets the shared JSON serializer options used by the Clientus API client.
    /// </summary>
    public static JsonSerializerOptions SerializerOptions => Options;
}
