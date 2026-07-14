using Clientus.ApiClient.Http;

namespace Clientus.ApiClient.Customers;

/// <summary>
/// Provides operations for managing customers.
/// </summary>
public class CustomersService
{
    private readonly ClientusHttpClient _http;

    /// <summary>
    /// Initializes a new instance of the CustomersService class.
    /// </summary>
    /// <param name="http">HTTP client used to communicate with Clientus.</param>
    public CustomersService(ClientusHttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Retrieves customers.
    /// </summary>
    /// <param name="limit">
    /// Maximum number of customers to return.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// A list of customers.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the underlying client has been disposed.
    /// </exception>
    public async Task<IReadOnlyList<Customer>> GetAllAsync(
     int? limit = null,
     CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();

        if (limit is <= 0)
            throw new ArgumentOutOfRangeException(nameof(limit));

        var endpoint =
    "/rest/v1/clients" +
    "?select=id,company_id,first_name,last_name,display_name,email,phone,address,postal_code,city,country,client_type,created_at" +
    "&order=created_at.desc";

        if (limit.HasValue)
        {
            endpoint += $"&limit={limit.Value}";
        }

        var customers =
            await _http.GetAsync<List<Customer>>(
                endpoint,
                cancellationToken);

        return customers ?? new List<Customer>();
    }

    /// <summary>
    /// Retrieves a customer by its identifier.
    /// </summary>
    /// <param name="id">
    /// Customer identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// The customer if found; otherwise <c>null</c>.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the underlying client has been disposed.
    /// </exception>
    public async Task<Customer?> GetByIdAsync(
    string id,
    CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException(
                "L'ID cliente è obbligatorio.",
                nameof(id));

        var endpoint =
            "/rest/v1/clients" +
            "?select=*" +
            $"&id=eq.{Uri.EscapeDataString(id)}" +
            "&limit=1";

        var result =
            await _http.GetAsync<List<Customer>>(
                endpoint,
                cancellationToken);

        return result?.FirstOrDefault();
    }

    /// <summary>
    /// Searches customers by name or email.
    /// </summary>
    /// <param name="text">
    /// Search text.
    /// </param>
    /// <param name="limit">
    /// Maximum number of results.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// Matching customers.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the underlying client has been disposed.
    /// </exception>
    public async Task<IReadOnlyList<Customer>> SearchAsync(
    string text,
    int limit = 20,
    CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<Customer>();

        if (limit <= 0)
            throw new ArgumentOutOfRangeException(nameof(limit));

        var search =
            Uri.EscapeDataString($"*{text.Trim()}*");

        var endpoint =
            "/rest/v1/clients" +
            "?select=id,company_id,first_name,last_name,display_name,email,phone,address,postal_code,city,country,client_type,created_at" +
            $"&or=(first_name.ilike.{search},last_name.ilike.{search},display_name.ilike.{search},email.ilike.{search},phone.ilike.{search})" +
            "&order=display_name.asc.nullslast,first_name.asc" +
            $"&limit={limit}";

        var customers =
            await _http.GetAsync<List<Customer>>(
                endpoint,
                cancellationToken);

        return customers ?? new List<Customer>();
    }
}
