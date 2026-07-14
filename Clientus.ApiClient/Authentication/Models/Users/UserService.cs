using Clientus.ApiClient.Http;
using Clientus.ApiClient.Users.Models;

namespace Clientus.ApiClient.Users;

public class UserService
{
    private readonly ClientusHttpClient _http;

    public UserService(ClientusHttpClient http)
    {
        _http = http;
    }

    public async Task<User?> GetCurrentAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException(
                "L'ID utente è obbligatorio.",
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

    public async Task<IReadOnlyList<User>> SearchAsync(
    string text,
    int limit = 20,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<User>();

        if (limit <= 0)
            throw new ArgumentOutOfRangeException(nameof(limit));

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
