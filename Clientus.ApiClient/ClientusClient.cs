using Clientus.ApiClient.Ai;
using Clientus.ApiClient.Authentication;
using Clientus.ApiClient.BusinessUnits;
using Clientus.ApiClient.Catalog;
using Clientus.ApiClient.Configuration;
using Clientus.ApiClient.CustomerIntelligence;
using Clientus.ApiClient.Customers;
using Clientus.ApiClient.Http;
using Clientus.ApiClient.Invoices;
using Clientus.ApiClient.Loyalty;
using Clientus.ApiClient.PublicCommerce;
using Clientus.ApiClient.Publishing;
using Clientus.ApiClient.Quotes;
using Clientus.ApiClient.RetailDistribution;
using Clientus.ApiClient.RetailIntelligence;
using Clientus.ApiClient.RetailReturns;
using Clientus.ApiClient.Users;
using Clientus.ApiClient.Warehouse;
using Clientus.ApiClient.Website;

namespace Clientus.ApiClient;

/// <summary>
/// Provides the main entry point for authenticated Clientus API services.
/// </summary>
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

    /// <summary>Gets the Clientus Intelligence / Lumi Developer API service.</summary>
    public AiService Ai { get; }

    /// <summary>Gets read-only Business Units operations.</summary>
    public BusinessUnitsService BusinessUnits { get; }
    /// <summary>Gets read-only Warehouse operations.</summary>
    public WarehouseService Warehouse { get; }
    /// <summary>Gets read-only Loyalty operations.</summary>
    public LoyaltyService Loyalty { get; }
    /// <summary>Gets read-only Customer Intelligence operations.</summary>
    public CustomerIntelligenceService CustomerIntelligence { get; }
    /// <summary>Gets read-only Retail Returns operations.</summary>
    public RetailReturnsService RetailReturns { get; }
    /// <summary>Gets read-only Retail Distribution operations.</summary>
    public RetailDistributionService RetailDistribution { get; }
    /// <summary>Gets read-only Advanced Retail Intelligence operations.</summary>
    public RetailIntelligenceService RetailIntelligence { get; }
    /// <summary>Gets read-only Publishing operations.</summary>
    public PublishingService Publishing { get; }
    /// <summary>Gets read-only Website operations.</summary>
    public WebsiteService Website { get; }
    /// <summary>Gets read-only Public Commerce operations.</summary>
    public PublicCommerceService PublicCommerce { get; }

    public AuthService Auth { get { ThrowIfDisposed(); return _auth; } }
    public CatalogService Catalog { get { ThrowIfDisposed(); _catalog.ThrowIfDisposed(); return _catalog; } }
    public CustomersService Customers { get { ThrowIfDisposed(); return _customers; } }
    public QuotesService Quotes { get { ThrowIfDisposed(); _quotes.ThrowIfDisposed(); return _quotes; } }
    public InvoicesService Invoices { get { ThrowIfDisposed(); _invoices.ThrowIfDisposed(); return _invoices; } }
    public UserService Users { get { ThrowIfDisposed(); return _users; } }

    public ClientusClient(ClientusConfiguration configuration)
    {
        _http = new ClientusHttpClient(configuration);
        Ai = new AiService(_http);

        _auth = new AuthService(_http);
        _catalog = new CatalogService(_http);
        _customers = new CustomersService(_http);
        _quotes = new QuotesService(_http);
        _invoices = new InvoicesService(_http);
        _users = new UserService(_http);

        BusinessUnits = new BusinessUnitsService(_http);
        Warehouse = new WarehouseService(_http);
        Loyalty = new LoyaltyService(_http);
        CustomerIntelligence = new CustomerIntelligenceService(_http);
        RetailReturns = new RetailReturnsService(_http);
        RetailDistribution = new RetailDistributionService(_http);
        RetailIntelligence = new RetailIntelligenceService(_http);
        Publishing = new PublishingService(_http);
        Website = new WebsiteService(_http);
        PublicCommerce = new PublicCommerceService(_http);
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
            _http.Dispose();
    }

    private void ThrowIfDisposed() =>
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
}
