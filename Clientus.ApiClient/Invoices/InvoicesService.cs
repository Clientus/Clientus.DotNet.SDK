using Clientus.ApiClient.Http;

namespace Clientus.ApiClient.Invoices;

/// <summary>Read-only invoice operations exposed by Clientus Public API v1.</summary>
public class InvoicesService
{
    private readonly IClientusApiTransport _http;
    public InvoicesService(ClientusHttpClient http) : this((IClientusApiTransport)http) { }
    internal InvoicesService(IClientusApiTransport http)
    {
        ArgumentNullException.ThrowIfNull(http);
        _http = http;
    }

    public Task<Invoice?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        ValidateId(id);
        return _http.GetAsync<Invoice>($"/api/v1/invoices/{Uri.EscapeDataString(id)}", cancellationToken);
    }

    public async Task<IReadOnlyList<Invoice>> ListAsync(CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        return await _http.GetAsync<List<Invoice>>("/api/v1/invoices", cancellationToken) ?? [];
    }

    [Obsolete("Invoice item expansion is not yet exposed by Public API v1.")]
    public Task<InvoiceWithItems?> GetWithItemsAsync(string id, CancellationToken cancellationToken = default) =>
        Unsupported<InvoiceWithItems?>(cancellationToken, "Invoice item expansion");

    [Obsolete("Invoice existence probes are not yet exposed by Public API v1.")]
    public Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default) =>
        Unsupported<bool>(cancellationToken, "Invoice existence");

    [Obsolete("Invoice exact counts are not yet exposed by Public API v1.")]
    public Task<long> CountAsync(CancellationToken cancellationToken = default) =>
        Unsupported<long>(cancellationToken, "Invoice count");

    [Obsolete("Invoice deletion is not yet exposed by Public API v1.")]
    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        throw new NotSupportedException("Invoice deletion is not available in Clientus Public API v1.");
    }

    internal void ThrowIfDisposed() => _http.ThrowIfDisposed();
    private static void ValidateId(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("An invoice identifier is required.", nameof(id));
    }
    private static Task<T> Unsupported<T>(CancellationToken token, string operation)
    {
        token.ThrowIfCancellationRequested();
        throw new NotSupportedException($"{operation} is not available in Clientus Public API v1.");
    }
}