using Clientus.ApiClient.Authentication.Models;
using Clientus.ApiClient.Http;

namespace Clientus.ApiClient.Authentication;

/// <summary>
/// Compatibility surface for the retired end-user authentication flow.
/// Public API v1 uses a developer credential configured on <see cref="ClientusClient"/>.
/// </summary>
public class AuthService
{
    private readonly IClientusApiTransport _http;

    public AuthService(ClientusHttpClient http) : this((IClientusApiTransport)http) { }

    internal AuthService(IClientusApiTransport http)
    {
        ArgumentNullException.ThrowIfNull(http);
        _http = http;
    }

    /// <summary>Human sessions are not part of the Public API v1 developer-credential contract.</summary>
    public AuthSession? CurrentSession
    {
        get { _http.ThrowIfDisposed(); return null; }
    }

    /// <summary>Always false because developer credentials are not human login sessions.</summary>
    public bool IsAuthenticated
    {
        get { _http.ThrowIfDisposed(); return false; }
    }

    [Obsolete("Email/password login is not supported by the Public API v1 SDK. Use DeveloperCredential.")]
    public Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new LoginResponse
        {
            Success = false,
            Error = "Human email/password authentication is not supported by the Clientus Public API v1 SDK."
        });
    }

    [Obsolete("Session refresh is not supported by the Public API v1 SDK.")]
    public Task<LoginResponse> RefreshAsync(CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new LoginResponse
        {
            Success = false,
            Error = "Human session refresh is not supported by the Clientus Public API v1 SDK."
        });
    }

    [Obsolete("Human authenticated-user lookup is not supported by the Public API v1 SDK. Use Users.GetCurrentAsync().")]
    public Task<AuthUser?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<AuthUser?>(null);
    }

    [Obsolete("Human logout is not applicable to developer credentials.")]
    public Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}