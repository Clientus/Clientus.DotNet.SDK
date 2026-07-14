using Clientus.ApiClient.Authentication;
using Clientus.ApiClient.Configuration;
using Clientus.ApiClient.Customers;
using Clientus.ApiClient.Http;
using Clientus.ApiClient.Users;


namespace Clientus.ApiClient;

/// <summary>
/// Provides the main entry point for accessing the Clientus API.
/// </summary>
public class ClientusClient
{
    private readonly ClientusHttpClient _http;

    /// <summary>
    /// Gets the authentication service.
    /// </summary>
    public AuthService Auth { get; }

    /// <summary>
    /// Gets the customers service.
    /// </summary>
    public CustomersService Customers { get; }

    /// <summary>
    /// Gets the users service.
    /// </summary>
    public UserService Users { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientusClient"/> class.
    /// </summary>
    /// <param name="configuration">
    /// The configuration used to connect to the Clientus API.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configuration"/> is null.
    /// </exception>
    public ClientusClient(ClientusConfiguration configuration)
    {
        _http = new ClientusHttpClient(configuration);

        Auth = new AuthService(_http);
        Customers = new CustomersService(_http);
        Users = new UserService(_http);
    }
}