using System.Text.Json;
using Clientus.ApiClient.Serialization;

namespace Clientus.ApiClient.Http;

/// <summary>Normalizes the versioned Public API response envelope without exposing transport details.</summary>
internal static class PublicApiJson
{
    public static T? Deserialize<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (root.ValueKind == JsonValueKind.Object)
        {
            if (root.TryGetProperty("data", out var data))
                return data.Deserialize<T>(JsonHelper.SerializerOptions);

            // List envelopes may use "items" as the payload key.
            if (root.TryGetProperty("items", out var items))
                return items.Deserialize<T>(JsonHelper.SerializerOptions);
        }

        return root.Deserialize<T>(JsonHelper.SerializerOptions);
    }
}