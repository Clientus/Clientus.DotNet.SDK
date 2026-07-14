using Clientus.ApiClient.Http;
using Clientus.ApiClient.Users.Models;
using Clientus.ApiClient.Common;

namespace Clientus.ApiClient.Users;

/// <summary>
/// Provides operations for retrieving and searching Clientus users.
/// </summary>
public class UserService
{
    private readonly ClientusHttpClient _http;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserService"/> class.
    /// </summary>
    /// <param name="http">
    /// HTTP client used to communicate with the Clientus API.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="http"/> is <see langword="null"/>.
    /// </exception>
    public UserService(ClientusHttpClient http)
    {
        ArgumentNullException.ThrowIfNull(http);
        _http = http;
        
    }

    /// <summary>
    /// Retrieves a user by identifier.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the user to retrieve.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The matching user, or <see langword="null"/> when no user is found.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="userId"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="ApiException">
    /// Thrown when the API returns an unsuccessful HTTP status code.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the underlying client has been disposed.
    /// </exception>
    public async Task<User?> GetCurrentAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException(
                "User Id is required.",
                nameof(userId));
        }

        var encodedUserId = Uri.EscapeDataString(userId);

        var endpoint =
    "/rest/v1/profiles" +
    "?select=user_id,username,full_name,avatar_url,account_type,account_status,approval_status,demo_mode,is_beta_tester,created_at,updated_at" +
    $"&user_id=eq.{encodedUserId}" +
    "&limit=1";

        var users = await _http.GetAsync<List<User>>(
            endpoint,
            cancellationToken);

        return users?.FirstOrDefault();
    }

    /// <summary>
    /// Searches users by the specified text.
    /// </summary>
    /// <param name="text">
    /// The text used to search users.
    /// </param>
    /// <param name="limit">
    /// The maximum number of users to return.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A read-only list containing the matching users.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="limit"/> is less than or equal to zero.
    /// </exception>
    /// <exception cref="ApiException">
    /// Thrown when the API returns an unsuccessful HTTP status code.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the underlying client has been disposed.
    /// </exception>
    public async Task<IReadOnlyList<User>> SearchAsync(
    string text,
    int limit = 20,
    CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<User>();

        if (limit <= 0)
            throw new ArgumentOutOfRangeException(
    nameof(limit),
    "Limit must be greater than zero.");

        var search = Uri.EscapeDataString($"*{text.Trim()}*");

        var endpoint =
    "/rest/v1/profiles" +
    "?select=user_id,username,full_name,avatar_url,account_type,account_status,approval_status,demo_mode,is_beta_tester,created_at,updated_at" +
    $"&or=(username.ilike.{search},full_name.ilike.{search})" +
    "&order=full_name.asc.nullslast,username.asc" +
    $"&limit={limit}";

        var users = await _http.GetAsync<List<User>>(
            endpoint,
            cancellationToken);

        return users ?? new List<User>();
    }
}
