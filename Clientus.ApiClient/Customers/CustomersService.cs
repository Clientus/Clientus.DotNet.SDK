using Clientus.ApiClient.Http;

namespace Clientus.ApiClient.Customers;

/// <summary>
/// Provides authenticated customer operations whose visibility is controlled by row-level security.
/// </summary>
public class CustomersService
{
    private readonly ClientusHttpClient _http;

    /// <summary>
    /// Initializes a new instance of the CustomersService class.
    /// </summary>
    /// <param name="http">HTTP client used to communicate with Clientus.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="http"/> is null.</exception>
    public CustomersService(ClientusHttpClient http)
    {
        ArgumentNullException.ThrowIfNull(http);
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

        return PostgRestQuery.OrEmpty(customers);
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

        var endpoint =
            "/rest/v1/clients" +
            "?select=*" +
            $"&{PostgRestQuery.ExactFilter("id", id, nameof(id))}" +
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

        return PostgRestQuery.OrEmpty(customers);
    }

    /// <summary>
    /// Updates the specified customer.
    /// </summary>
    /// <param name="customer">The customer values to update. The identifier selects the customer and is not updated.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The updated customer.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="customer"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the customer identifier is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no customer is returned after the update.
    /// </exception>
    /// <exception cref="Clientus.ApiClient.Common.ApiException">
    /// Thrown when the API returns an unsuccessful HTTP status code.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the underlying client has been disposed.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when cancellation is requested through <paramref name="cancellationToken"/>.
    /// </exception>
    public async Task<Customer> UpdateAsync(
        Customer customer,
        CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(customer);

        var endpoint =
            "/rest/v1/clients" +
            $"?{PostgRestQuery.ExactFilter("id", customer.Id, nameof(customer))}" +
            "&select=*";

        var updated = await _http.PatchAsync<List<Customer>>(
            endpoint,
            new
            {
                first_name = customer.FirstName,
                last_name = customer.LastName,
                display_name = customer.DisplayName,
                email = customer.Email,
                phone = customer.Phone,
                address = customer.Address,
                postal_code = customer.PostalCode,
                city = customer.City,
                country = customer.Country,
                client_type = customer.ClientType
            },
            cancellationToken);

        return updated?.SingleOrDefault()
            ?? throw new InvalidOperationException(
                "The API did not return the updated customer.");
    }

    /// <summary>
    /// Deletes the customer with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the customer to delete.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="id"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="Clientus.ApiClient.Common.ApiException">
    /// Thrown when the API returns an unsuccessful HTTP status code.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the underlying client has been disposed.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when cancellation is requested through <paramref name="cancellationToken"/>.
    /// </exception>
    public async Task DeleteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();

        var endpoint =
            "/rest/v1/clients" +
            $"?{PostgRestQuery.ExactFilter("id", id, nameof(id))}";

        await _http.DeleteAsync(endpoint, cancellationToken);
    }

    /// <summary>
    /// Determines whether a customer with the specified identifier exists.
    /// </summary>
    /// <param name="id">The customer identifier.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns><see langword="true"/> when the customer exists; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="id"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="Clientus.ApiClient.Common.ApiException">
    /// Thrown when the API returns an unsuccessful HTTP status code.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the underlying client has been disposed.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when cancellation is requested through <paramref name="cancellationToken"/>.
    /// </exception>
    public async Task<bool> ExistsAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();

        var endpoint =
            "/rest/v1/clients" +
            "?select=id" +
            $"&{PostgRestQuery.ExactFilter("id", id, nameof(id))}" +
            "&limit=1";

        var matches = await _http.GetAsync<List<object>>(
            endpoint,
            cancellationToken);

        return matches?.Count > 0;
    }

    /// <summary>
    /// Gets the exact number of customers visible to the current user.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The exact number of customers allowed by row-level security.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when PostgREST does not return an exact count.
    /// </exception>
    /// <exception cref="Clientus.ApiClient.Common.ApiException">
    /// Thrown when the API returns an unsuccessful HTTP status code.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the underlying client has been disposed.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when cancellation is requested through <paramref name="cancellationToken"/>.
    /// </exception>
    public Task<long> CountAsync(
        CancellationToken cancellationToken = default)
    {
        _http.ThrowIfDisposed();

        return _http.HeadCountAsync(
            "/rest/v1/clients?select=id",
            cancellationToken);
    }
}
