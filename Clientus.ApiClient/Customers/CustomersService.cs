using Clientus.ApiClient.Http;

namespace Clientus.ApiClient.Customers;

/// <summary>Read-only customer operations exposed by Clientus Public API v1.</summary>
public class CustomersService
{
    private readonly IClientusApiTransport _http;
    public CustomersService(ClientusHttpClient http) : this((IClientusApiTransport)http) { }
    internal CustomersService(IClientusApiTransport http)
    {
        ArgumentNullException.ThrowIfNull(http);
        _http = http;
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync(
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        if (limit is <= 0 or > 100) throw new ArgumentOutOfRangeException(nameof(limit));
        var endpoint = "/api/v1/customers" + (limit.HasValue ? $"?limit={limit.Value}" : string.Empty);
        return await _http.GetAsync<List<Customer>>(endpoint, cancellationToken) ?? [];
    }

    public Task<Customer?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        ValidateId(id);
        return _http.GetAsync<Customer>($"/api/v1/customers/{Uri.EscapeDataString(id)}", cancellationToken);
    }

    [Obsolete("Customer search is not yet exposed by the Public API v1 contract.")]
    public Task<IReadOnlyList<Customer>> SearchAsync(string text, int limit = 20, CancellationToken cancellationToken = default) =>
        UnsupportedList(cancellationToken, "Customer search");

    [Obsolete("Customer mutation is not yet exposed by the Public API v1 contract.")]
    public Task<Customer> UpdateAsync(Customer customer, CancellationToken cancellationToken = default) =>
        Unsupported<Customer>(cancellationToken, "Customer update");

    [Obsolete("Customer deletion is not yet exposed by the Public API v1 contract.")]
    public Task DeleteAsync(string id, CancellationToken cancellationToken = default) =>
        Unsupported(cancellationToken, "Customer deletion");

    [Obsolete("Customer existence probes are not yet exposed by the Public API v1 contract.")]
    public Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default) =>
        Unsupported<bool>(cancellationToken, "Customer existence");

    [Obsolete("Customer exact counts are not yet exposed by the Public API v1 contract.")]
    public Task<long> CountAsync(CancellationToken cancellationToken = default) =>
        Unsupported<long>(cancellationToken, "Customer count");

    private static void ValidateId(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A customer identifier is required.", nameof(id));
    }

    private static Task<T> Unsupported<T>(CancellationToken token, string operation)
    {
        token.ThrowIfCancellationRequested();
        throw new NotSupportedException($"{operation} is not available in Clientus Public API v1.");
    }
    private static Task Unsupported(CancellationToken token, string operation)
    {
        token.ThrowIfCancellationRequested();
        throw new NotSupportedException($"{operation} is not available in Clientus Public API v1.");
    }
    private static Task<IReadOnlyList<Customer>> UnsupportedList(CancellationToken token, string operation)
    {
        token.ThrowIfCancellationRequested();
        throw new NotSupportedException($"{operation} is not available in Clientus Public API v1.");
    }
}