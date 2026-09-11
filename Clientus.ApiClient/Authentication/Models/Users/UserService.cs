using Clientus.ApiClient.Common;
using Clientus.ApiClient.Http;
using Clientus.ApiClient.Users.Models;

namespace Clientus.ApiClient.Users;

/// <summary>Developer-visible current-context profile operations.</summary>
public class UserService
{
    private readonly IClientusApiTransport _http;
    public UserService(ClientusHttpClient http) : this((IClientusApiTransport)http) { }
    internal UserService(IClientusApiTransport http)
    {
        ArgumentNullException.ThrowIfNull(http);
        _http = http;
    }

    /// <summary>Gets the profile/context visible to the current developer installation.</summary>
    public Task<User?> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        return _http.GetAsync<User>("/api/v1/me", cancellationToken);
    }

    /// <summary>
    /// Compatibility overload. Public API v1 never permits selecting an arbitrary user; it reads
    /// only the current authorized context and returns it only when the identifier matches.
    /// </summary>
    [Obsolete("Use GetCurrentAsync(CancellationToken). Public API v1 does not expose arbitrary user lookup.")]
    public async Task<User?> GetCurrentAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("A user identifier is required.", nameof(userId));
        var current = await GetCurrentAsync(cancellationToken);
        return string.Equals(current?.UserId, userId, StringComparison.Ordinal) ? current : null;
    }

    [Obsolete("User search is not exposed by Clientus Public API v1.")]
    public Task<IReadOnlyList<User>> SearchAsync(string text, int limit = 20, CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        throw new NotSupportedException("User search is not available in Clientus Public API v1.");
    }
}