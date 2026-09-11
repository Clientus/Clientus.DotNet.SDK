using Clientus.ApiClient;
using Clientus.ApiClient.Common;
using Clientus.ApiClient.Configuration;

var baseUrl = RequireEnvironmentVariable("CLIENTUS_BASE_URL");
var credential = RequireEnvironmentVariable("CLIENTUS_DEVELOPER_CREDENTIAL");

using var client = new ClientusClient(new ClientusConfiguration
{
    BaseUrl = baseUrl,
    DeveloperCredential = credential,
    Environment = ClientusEnvironment.Sandbox
});

using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    var me = await client.Users.GetCurrentAsync(cancellationSource.Token);
    Console.WriteLine($"Connected to Clientus as: {me?.Username ?? me?.FullName ?? "developer context"}");

    var customers = await client.Customers.GetAllAsync(
        limit: 50,
        cancellationToken: cancellationSource.Token);

    Console.WriteLine($"Customers returned: {customers.Count}");
    foreach (var customer in customers)
        Console.WriteLine($"- {customer.FullName} ({customer.Email ?? "no email"})");
}
catch (ApiException exception)
{
    Console.Error.WriteLine(
        $"Clientus API error {(int)exception.StatusCode} [{exception.ErrorCode}] request={exception.RequestId}: {exception.Message}");
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("The operation was cancelled or timed out.");
}

static string RequireEnvironmentVariable(string name) =>
    Environment.GetEnvironmentVariable(name) is { Length: > 0 } value
        ? value
        : throw new InvalidOperationException($"Set {name} before running the example.");