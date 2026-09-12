using System.Text.Json;
using Clientus.ApiClient.Http;

namespace Clientus.ApiClient.Ai;

/// <summary>
/// Clientus Intelligence / Lumi Developer API.
/// Provider selection and provider credentials remain server-side.
/// </summary>
public sealed class AiService
{
    private readonly ClientusHttpClient _http;

    internal AiService(ClientusHttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
    }

    public Task<AiResponse<JsonElement>> ExecuteAsync(
        AiRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _http.PostAsync<AiResponse<JsonElement>>(
            "/api/v1/ai",
            request,
            cancellationToken);
    }
}