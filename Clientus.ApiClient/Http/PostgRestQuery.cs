namespace Clientus.ApiClient.Http;

/// <summary>Shared, internal PostgREST query operations used by concrete SDK services.</summary>
internal static class PostgRestQuery
{
    internal static string ExactFilter(string column, string value, string parameterName)
    {
        ValidateIdentifier(value, parameterName);
        return $"{column}=eq.{Uri.EscapeDataString(value)}";
    }

    internal static void ValidateIdentifier(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("An identifier is required.", parameterName);
        }
    }

    internal static IReadOnlyList<T> OrEmpty<T>(List<T>? values) =>
        values is null ? Array.Empty<T>() : values.AsReadOnly();
}
