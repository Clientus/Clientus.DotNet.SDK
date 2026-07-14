using System.Text.Json.Serialization;

namespace Clientus.ApiClient.Authentication;

/// <summary>
/// Represents an authenticated Clientus session.
/// </summary>
public class AuthSession
{
    /// <summary>
    /// Gets or sets the JWT access token.
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the token type.
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the token lifetime in seconds.
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Indicates whether the session contains a valid access token.
    /// </summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(AccessToken);
}