using Clientus.ApiClient;
using Clientus.ApiClient.Authentication.Models;
using Clientus.ApiClient.Common;
using Clientus.ApiClient.Configuration;

var baseUrl = RequireEnvironmentVariable("CLIENTUS_BASE_URL");
var apiKey = RequireEnvironmentVariable("CLIENTUS_API_KEY");
var identifier = RequireEnvironmentVariable("CLIENTUS_IDENTIFIER");
var password = RequireEnvironmentVariable("CLIENTUS_PASSWORD");

using var client = new ClientusClient(new ClientusConfiguration
{
    BaseUrl = baseUrl,
    ApiKey = apiKey,
});
using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    var login = await client.Auth.LoginAsync(
        new LoginRequest { Identifier = identifier, Password = password },
        cancellationSource.Token);

    if (!login.Success)
    {
        Console.Error.WriteLine($"Authentication failed: {login.Error}");
        return;
    }

    var customers = await client.Customers.GetAllAsync(
        cancellationToken: cancellationSource.Token);

    Console.WriteLine($"Visible customers: {customers.Count}");
    foreach (var customer in customers)
        Console.WriteLine($"- {customer.FullName} ({customer.Email ?? "no email"})");
}
catch (ApiException exception)
{
    Console.Error.WriteLine($"Clientus API error {(int)exception.StatusCode}: {exception.ResponseBody}");
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("The operation was cancelled or timed out.");
}

static string RequireEnvironmentVariable(string name) =>
    Environment.GetEnvironmentVariable(name) is { Length: > 0 } value
        ? value
        : throw new InvalidOperationException($"Set the {name} environment variable before running the example.");
