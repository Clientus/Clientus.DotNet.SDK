using Clientus.ApiClient.Authentication;
using Clientus.ApiClient.Catalog;
using Clientus.ApiClient.Configuration;
using Clientus.ApiClient.Customers;
using Clientus.ApiClient.Http;
using Clientus.ApiClient.Invoices;
using Clientus.ApiClient.Quotes;
using Clientus.ApiClient.Users;


namespace Clientus.ApiClient;

/// <summary>
/// Provides the main entry point for authenticated Clientus API services.
/// </summary>
/// <remarks>
/// One shared HTTP transport is owned by this instance. Service properties return stable instances,
/// and disposing this client disposes the shared transport exactly once.
/// </remarks>
public class ClientusClient : IDisposable
{
    private readonly ClientusHttpClient _http;
    private readonly AuthService _auth;
    private readonly CatalogService _catalog;
    private readonly CustomersService _customers;
    private readonly QuotesService _quotes;
    private readonly InvoicesService _invoices;
    private readonly UserService _users;
    private int _disposed;

    /// <summary>
    /// Gets the authentication service.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when this instance has been disposed.
    /// </exception>
    public AuthService Auth
    {
        get
        {
            ThrowIfDisposed();
            return _auth;
        }
    }

    /// <summary>Gets the authenticated, RLS-controlled catalog service.</summary>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    public CatalogService Catalog
    {
        get
        {
            ThrowIfDisposed();
            _catalog.ThrowIfDisposed();
            return _catalog;
        }
    }

    /// <summary>
    /// Gets the customers service.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when this instance has been disposed.
    /// </exception>
    public CustomersService Customers
    {
        get
        {
            ThrowIfDisposed();
            return _customers;
        }
    }

    /// <summary>
    /// Gets the quotes service.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when this instance has been disposed.
    /// </exception>
    public QuotesService Quotes
    {
        get
        {
            ThrowIfDisposed();
            _quotes.ThrowIfDisposed();
            return _quotes;
        }
    }

    /// <summary>Gets the authenticated, RLS-controlled invoices service.</summary>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    public InvoicesService Invoices
    {
        get
        {
            ThrowIfDisposed();
            _invoices.ThrowIfDisposed();
            return _invoices;
        }
    }

    /// <summary>
    /// Gets the users service.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when this instance has been disposed.
    /// </exception>
    public UserService Users
    {
        get
        {
            ThrowIfDisposed();
            return _users;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientusClient"/> class.
    /// </summary>
    /// <param name="configuration">
    /// The configuration used to connect to the Clientus API.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configuration"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">Thrown when the base URL or API key is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when timeout or retry settings are invalid.</exception>
    public ClientusClient(ClientusConfiguration configuration)
    {
        _http = new ClientusHttpClient(configuration);

        _auth = new AuthService(_http);
        _catalog = new CatalogService(_http);
        _customers = new CustomersService(_http);
        _quotes = new QuotesService(_http);
        _invoices = new InvoicesService(_http);
        _users = new UserService(_http);
    }

    /// <summary>Disposes the shared HTTP transport. Repeated calls are safe.</summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
        {
            _http.Dispose();
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(
            Volatile.Read(ref _disposed) != 0,
            this);
    }
}
