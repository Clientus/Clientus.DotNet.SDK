namespace Clientus.ApiClient.Authentication.Models;

/// <summary>
/// Represents a Clientus authentication request.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Gets or sets the email address used for legacy authentication.
    /// Username authentication is no longer supported by the legacy backend contract.
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the session should be remembered.
    /// </summary>
    public bool RememberMe { get; set; } = true;
}
