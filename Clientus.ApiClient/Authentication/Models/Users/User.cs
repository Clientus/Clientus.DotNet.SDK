using System.Text.Json.Serialization;

namespace Clientus.ApiClient.Users.Models;

public class User
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("full_name")]
    public string? FullName { get; set; }

    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }

    [JsonPropertyName("account_type")]
    public string? AccountType { get; set; }

    [JsonPropertyName("account_status")]
    public string? AccountStatus { get; set; }

    [JsonPropertyName("approval_status")]
    public string? ApprovalStatus { get; set; }

    [JsonPropertyName("demo_mode")]
    public bool DemoMode { get; set; }

    [JsonPropertyName("is_beta_tester")]
    public bool IsBetaTester { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}