namespace Clientus.ApiClient.Configuration;

/// <summary>Clientus Public API environment selected by the SDK consumer.</summary>
public enum ClientusEnvironment
{
    /// <summary>Developer sandbox environment.</summary>
    Sandbox = 0,

    /// <summary>Live environment. The server still requires explicit live eligibility.</summary>
    Live = 1
}

/// <summary>Configuration used to connect to Clientus Public API v1.</summary>
public class ClientusConfiguration
{
    /// <summary>Gets or sets the Clientus API base URL.</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the opaque developer credential issued by Clientus.
    /// Store this only in server-side secret storage and never in browser/mobile source code.
    /// </summary>
    public string DeveloperCredential { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the intended Clientus environment. Credentials remain server-bound to their
    /// actual environment; this value does not elevate or convert a credential.
    /// </summary>
    public ClientusEnvironment Environment { get; set; } = ClientusEnvironment.Sandbox;

    /// <summary>
    /// Legacy Supabase publishable keys are no longer accepted by the Public API v1 transport.
    /// This property remains only for source compatibility during the beta transition.
    /// </summary>
    [Obsolete("ApiKey is no longer used. Configure DeveloperCredential for Clientus Public API v1.")]
    public string ApiKey { get; set; } = string.Empty;

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public int MaxRetryAttempts { get; set; } = 3;
    public TimeSpan InitialRetryDelay { get; set; } = TimeSpan.FromMilliseconds(500);
    public TimeSpan MaximumRetryDelay { get; set; } = TimeSpan.FromSeconds(30);
    public HttpMessageHandler? HttpMessageHandler { get; set; }
    public bool DisposeHttpMessageHandler { get; set; } = true;
}