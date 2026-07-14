namespace Clientus.ApiClient.Authentication.Models;

public class LoginResponse
{
    public bool Success { get; set; }

    public string? Error { get; set; }

    public AuthSession? Session { get; set; }
}