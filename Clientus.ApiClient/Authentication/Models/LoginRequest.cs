namespace Clientus.ApiClient.Authentication.Models;

public class LoginRequest
{
    /// <summary>
    /// Username oppure Email.
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// Password.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Mantieni la sessione.
    /// </summary>
    public bool RememberMe { get; set; } = true;
}