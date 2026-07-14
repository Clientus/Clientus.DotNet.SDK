using System.Text.Json.Serialization;

namespace Clientus.ApiClient.Customers;

/// <summary>
/// Represents a customer in Clientus.
/// </summary>
public class Customer
{
    /// <summary>
    /// Customer identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Company identifier.
    /// </summary>
    [JsonPropertyName("company_id")]
    public string CompanyId { get; set; } = string.Empty;

    /// <summary>
    /// First name.
    /// </summary>
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name.
    /// </summary>
    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    /// <summary>
    /// Display name.
    /// </summary>
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Email address.
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Phone number.
    /// </summary>
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    /// <summary>
    /// Postal address.
    /// </summary>
    [JsonPropertyName("address")]
    public string? Address { get; set; }

    /// <summary>
    /// Postal code.
    /// </summary>
    [JsonPropertyName("postal_code")]
    public string? PostalCode { get; set; }

    /// <summary>
    /// City.
    /// </summary>
    [JsonPropertyName("city")]
    public string? City { get; set; }

    /// <summary>
    /// Country.
    /// </summary>
    [JsonPropertyName("country")]
    public string? Country { get; set; }

    /// <summary>
    /// Customer type.
    /// </summary>
    [JsonPropertyName("client_type")]
    public string ClientType { get; set; } = string.Empty;

    /// <summary>
    /// Creation date.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets the customer's full name.
    /// </summary>
    public string FullName =>
        !string.IsNullOrWhiteSpace(DisplayName)
            ? DisplayName
            : string.Join(
                " ",
                new[] { FirstName, LastName }
                    .Where(value => !string.IsNullOrWhiteSpace(value)));
}