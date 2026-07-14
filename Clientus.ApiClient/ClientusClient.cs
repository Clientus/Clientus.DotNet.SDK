using Clientus.ApiClient.Authentication;
using Clientus.ApiClient.Configuration;
using Clientus.ApiClient.Customers;
using Clientus.ApiClient.Http;
using Clientus.ApiClient.Users;


namespace Clientus.ApiClient;

public class ClientusClient
{
    private readonly ClientusHttpClient _http;

    public AuthService Auth { get; }

    public CustomersService Customers { get; }
    public UserService Users { get; }

    public ClientusClient(ClientusConfiguration configuration)
    {
        _http = new ClientusHttpClient(configuration);

        Auth = new AuthService(_http);
        Customers = new CustomersService(_http);
        Users = new UserService(_http);
    }
}