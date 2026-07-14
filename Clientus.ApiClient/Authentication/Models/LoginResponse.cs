namespace Clientus.ApiClient.Authentication.Models;

/// <summary>
/// Represents the result of a Clientus authentication attempt.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether authentication succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets or sets the authentication error message.
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Gets or sets the authenticated session.
    /// </summary>
    public AuthSession? Session { get; init; }
}