using Clientus.ApiClient.Http;

namespace Clientus.ApiClient.Quotes;

/// <summary>
/// Provides quote operations for the Clientus API.
/// </summary>
public class QuotesService
{
    private readonly ClientusHttpClient _http;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuotesService"/> class.
    /// </summary>
    /// <param name="http">
    /// HTTP client used to communicate with the Clientus API.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="http"/> is <see langword="null"/>.
    /// </exception>
    public QuotesService(ClientusHttpClient http)
    {
        ArgumentNullException.ThrowIfNull(http);
        _http = http;
    }

    internal void ThrowIfDisposed()
    {
        _http.ThrowIfDisposed();
    }
}
