namespace Clientus.ApiClient.Http;

/// <summary>
/// Defines the internal transport boundary used by public SDK services.
/// </summary>
/// <remarks>
/// The current implementation uses the legacy Supabase contract. A future Public API v1
/// transport can implement this boundary without changing public service types.
/// </remarks>
internal interface IClientusApiTransport
{
    Task<T?> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default);
    Task<T?> PostAsync<T>(string endpoint, object body, CancellationToken cancellationToken = default);
    Task<T?> PatchAsync<T>(string endpoint, object body, CancellationToken cancellationToken = default);
    Task DeleteAsync(string endpoint, CancellationToken cancellationToken = default);
    Task<long> HeadCountAsync(string endpoint, CancellationToken cancellationToken = default);
    void SetAccessToken(string? accessToken);
    void ThrowIfDisposed();
}
