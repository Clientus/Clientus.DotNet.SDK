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

    /// <summary>
    /// Gets the current authenticated session.
    /// Returns <see langword="null"/> when no user is authenticated.
    /// </summary>
    public AuthSession? CurrentSession { get; private set; }


    /// <summary>
    /// Gets a value indicating whether a valid authenticated session is available.
    /// </summary>
    public bool IsAuthenticated =>
        CurrentSession?.IsValid == true;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    /// <param name="http">
    /// HTTP client used to communicate with the Clientus API.
    /// </param>
    public AuthService(ClientusHttpClient http)
    {
        _http = http;
    }


    /// <summary>
    /// Authenticates a user and creates a new session.
    /// </summary>
    /// <param name="request">Login request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The authentication response.</returns>
    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return new LoginResponse
            {
                Success = false,
                Error = "Richiesta di accesso non valida."
            };
        }

        var identifier = request.Identifier.Trim();

        if (string.IsNullOrWhiteSpace(identifier))
        {
            return new LoginResponse
            {
                Success = false,
                Error = "Inserisci username o email."
            };
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return new LoginResponse
            {
                Success = false,
                Error = "Inserisci la password."
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
                    Error = "Username o email non trovati."
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
                    Error = "Il server non ha restituito una sessione valida."
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
        catch (HttpRequestException exception)
        {
            return new LoginResponse
            {
                Success = false,
                Error = $"Accesso non riuscito: {exception.Message}"
            };
        }
        catch (Exception exception)
        {
            return new LoginResponse
            {
                Success = false,
                Error = $"Errore imprevisto durante l'accesso: {exception.Message}"
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
    /// Gets the currently authenticated user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The current authenticated user, or <see langword="null"/> if no session exists.</returns>

    public async Task<AuthUser?> GetCurrentUserAsync(
    CancellationToken cancellationToken = default)
    {
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

    public async Task LogoutAsync(
    CancellationToken cancellationToken = default)
    {
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