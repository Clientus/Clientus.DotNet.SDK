namespace Clientus.ApiClient.SdkNext;

internal static class SdkNextRead
{
    internal static string PageEndpoint(string route, int page, int pageSize, string? businessUnitId = null)
    {
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(pageSize));

        var endpoint = $"{route}?page={page}&pageSize={pageSize}";
        return businessUnitId is null
            ? endpoint
            : $"{endpoint}&businessUnitId={EscapedId(businessUnitId, nameof(businessUnitId))}";
    }

    internal static string DetailEndpoint(string route, string id, string? businessUnitId = null)
    {
        var endpoint = $"{route}/{EscapedId(id)}";
        return businessUnitId is null
            ? endpoint
            : $"{endpoint}?businessUnitId={EscapedId(businessUnitId, nameof(businessUnitId))}";
    }

    internal static string EscapedId(string id, string parameterName = "id")
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("A resource identifier is required.", parameterName);
        return Uri.EscapeDataString(id);
    }
}
