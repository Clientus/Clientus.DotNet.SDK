namespace Clientus.ApiClient.Configuration;

/// <summary>
/// Represents the configuration used to connect to the Clientus API.
/// </summary>
public class ClientusConfiguration
{
    /// <summary>
    /// Gets or sets the Clientus API base URL.
    /// The value must be an absolute HTTP or HTTPS URL.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the legacy Supabase publishable key.
    /// Never use a Supabase service-role key in an SDK application.
    /// This property will not become the future Public API developer-credential contract.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP request timeout.
    /// The value must be positive or <see cref="System.Threading.Timeout.InfiniteTimeSpan"/>.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for transient failures.
    /// The positive value includes the initial request.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the initial delay between retry attempts.
    /// The non-negative delay increases progressively for each retry.
    /// </summary>
    public TimeSpan InitialRetryDelay { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Gets or sets the maximum delay honored between safe-method retry attempts, including a
    /// server-supplied <c>Retry-After</c> value. The value must be non-negative.
    /// </summary>
    public TimeSpan MaximumRetryDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets an optional HTTP message handler used to construct the SDK-owned
    /// <see cref="HttpClient"/>. The handler is disposed with the SDK client by default.
    /// </summary>
    public HttpMessageHandler? HttpMessageHandler { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the optional <see cref="HttpMessageHandler"/>
    /// is disposed when the SDK client is disposed.
    /// </summary>
    public bool DisposeHttpMessageHandler { get; set; } = true;
}
