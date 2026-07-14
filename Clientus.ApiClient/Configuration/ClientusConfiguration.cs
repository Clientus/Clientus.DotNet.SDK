namespace Clientus.ApiClient.Configuration;

/// <summary>
/// Represents the configuration used to connect to the Clientus API.
/// </summary>
public class ClientusConfiguration
{
    /// <summary>
    /// Gets or sets the Clientus backend base URL.
    /// The value must be an absolute HTTP or HTTPS URL.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the public API key (Anon Key).
    /// Never use the Service Role Key in client applications.
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
}
