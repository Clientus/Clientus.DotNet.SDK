using System.Text.Json.Serialization;

namespace Clientus.ApiClient.Authentication;

public class AuthSession
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(AccessToken);
}