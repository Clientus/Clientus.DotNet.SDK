namespace Clientus.ApiClient.SdkNext;

internal static class SdkNextRead
{
    internal static string PageEndpoint(string route, int page, int pageSize)
    {
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(pageSize));
        return $"{route}?page={page}&pageSize={pageSize}";
    }

    internal static string EscapedId(string id, string parameterName = "id")
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("A resource identifier is required.", parameterName);
        return Uri.EscapeDataString(id);
    }
}
