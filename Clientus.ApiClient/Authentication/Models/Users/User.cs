using System.Text.Json.Serialization;

namespace Clientus.ApiClient.Users.Models;

/// <summary>
/// Represents a Clientus user.
/// </summary>
public class User
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the user's full name.
    /// </summary>
    [JsonPropertyName("full_name")]
    public string? FullName { get; set; }

    /// <summary>
    /// Gets or sets the avatar URL.
    /// </summary>
    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Gets or sets the account type.
    /// </summary>
    [JsonPropertyName("account_type")]
    public string? AccountType { get; set; }

    /// <summary>
    /// Gets or sets the account status.
    /// </summary>
    [JsonPropertyName("account_status")]
    public string? AccountStatus { get; set; }

    /// <summary>
    /// Gets or sets the approval status.
    /// </summary>
    [JsonPropertyName("approval_status")]
    public string? ApprovalStatus { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the account is in demo mode.
    /// </summary>
    [JsonPropertyName("demo_mode")]
    public bool DemoMode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is a beta tester.
    /// </summary>
    [JsonPropertyName("is_beta_tester")]
    public bool IsBetaTester { get; set; }

    /// <summary>
    /// Gets or sets the account creation date.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update date.
    /// </summary>
    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}