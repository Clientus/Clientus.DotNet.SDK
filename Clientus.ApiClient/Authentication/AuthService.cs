using System.Net.Http;
using System.Text.RegularExpressions;
using Clientus.ApiClient.Authentication.Models;
using Clientus.ApiClient.Http;

namespace Clientus.ApiClient.Authentication;

/// <summary>
/// Provides authentication operations for the Clientus API.
/// </summary>
public class AuthService
{
    private readonly ClientusHttpClient _http;
    private AuthSession? _currentSession;

    /// <summary>
    /// Gets the current authenticated session.
    /// Returns <see langword="null"/> when no user is authenticated.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the underlying client has been disposed.
    /// </exception>
    public AuthSession? CurrentSession
    {
        get
        {
            _http.ThrowIfDisposed();
            return _currentSession;
        }
        private set => _currentSession = value;
    }


    /// <summary>
    /// Gets a value indicating whether a valid authenticated session is available.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the underlying client has been disposed.
    /// </exception>
    public bool IsAuthenticated =>
        CurrentSession?.IsValid == true;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    /// <param name="http">
    /// HTTP client used to communicate with the Clientus API.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="http"/> is <see langword="null"/>.
    /// </exception>
    public AuthService(ClientusHttpClient http)
    {
        ArgumentNullException.ThrowIfNull(http);

        _http = http;
    }


    /// <summary>
    /// Authenticates a user and creates a new session.
    /// </summary>
    /// <param name="request">Login request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The authentication response.</returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown when cancellation is requested through <paramref name="cancellationToken"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the underlying client has been disposed.
    /// </exception>
    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();

        if (request is null)
        {
            return new LoginResponse
            {
                Success = false,
                Error = "Invalid access request."
            };
        }

        var identifier = request.Identifier.Trim();

        if (string.IsNullOrWhiteSpace(identifier))
        {
            return new LoginResponse
            {
                Success = false,
                Error = "Username or email is required."
            };
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return new LoginResponse
            {
                Success = false,
                Error = "Password is required."
            };
        }

        try
        {
            var email = await ResolveEmailAsync(
     identifier,
     cancellationToken);

            if (string.IsNullOrWhiteSpace(email))
            {
                return new LoginResponse
                {
                    Success = false,
                    Error = "Username or email was not found."
                };
            }

            var session = await _http.PostAsync<AuthSession>(
    "/auth/v1/token?grant_type=password",
    new
    {
        email,
        password = request.Password
    },
    cancellationToken);

            if (session?.IsValid != true)
            {
                return new LoginResponse
                {
                    Success = false,
                    Error = "Server did not return a valid session."
                };
            }

            CurrentSession = session;
            _http.SetAccessToken(session.AccessToken);

            return new LoginResponse
            {
                Success = true,
                Session = session
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            return new LoginResponse
            {
                Success = false,
                Error = $"Authentication failed: {exception.Message}"
            };
        }
        catch (Exception exception)
        {
            return new LoginResponse
            {
                Success = false,
                Error = $"Unexpected error during authentication: {exception.Message}"
            };
        }
    }

    private async Task<string?> ResolveEmailAsync(
    string identifier,
    CancellationToken cancellationToken = default)
    {
        if (IsEmail(identifier))
            return identifier.Trim().ToLowerInvariant();

        return await _http.PostAsync<string?>(
            "/rest/v1/rpc/email_for_username",
            new
            {
                _username = identifier.Trim()
            },
            cancellationToken);
    }

    private static bool IsEmail(string value)
    {
        return Regex.IsMatch(
            value.Trim(),
            @"^[^\s@]+@[^\s@]+\.[^\s@]+$",
            RegexOptions.CultureInvariant);
    }

    

    /// <summary>
    /// Refreshes the current authenticated session using its refresh token.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// The refreshed authentication response.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown when cancellation is requested through <paramref name="cancellationToken"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the underlying client has been disposed.
    /// </exception>
    public async Task<LoginResponse> RefreshAsync(
        CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();

        var refreshToken = CurrentSession?.RefreshToken?.Trim();

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return new LoginResponse
            {
                Success = false,
                Error = "No refresh token is available."
            };
        }

        try
        {
            var session = await _http.PostAsync<AuthSession>(
                "/auth/v1/token?grant_type=refresh_token",
                new
                {
                    refresh_token = refreshToken
                },
                cancellationToken);

            if (session?.IsValid != true)
            {
                return new LoginResponse
                {
                    Success = false,
                    Error = "The server did not return a valid session."
                };
            }

            CurrentSession = session;
            _http.SetAccessToken(session.AccessToken);

            return new LoginResponse
            {
                Success = true,
                Session = session
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            return new LoginResponse
            {
                Success = false,
                Error = $"Session update failed: {exception.Message}"
            };
        }
        catch (Exception exception)
        {
            return new LoginResponse
            {
                Success = false,
                Error = $"Unexpected error while updating the session: {exception.Message}"
            };
        }
    }

    /// <summary>
    /// Gets the currently authenticated user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The current authenticated user, or <see langword="null"/> if no session exists.</returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the underlying client has been disposed.
    /// </exception>
    public async Task<AuthUser?> GetCurrentUserAsync(
    CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();

        if (!IsAuthenticated)
            return null;

        return await _http.GetAsync<AuthUser>(
            "/auth/v1/user",
            cancellationToken);
    }

    /// <summary>
    /// Logs out the current user and clears the local session.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the underlying client has been disposed.
    /// </exception>
    public async Task LogoutAsync(
    CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();

        if (CurrentSession?.AccessToken is not null)
        {
            try
            {
                await _http.PostAsync<object?>(
                    "/auth/v1/logout",
                    new { },
                    cancellationToken);
            }
            catch
            {
                // Elimina comunque la sessione locale.
            }
        }

        CurrentSession = null;
        _http.SetAccessToken(null);
    }
}
