using Clientus.ApiClient.Configuration;

namespace Clientus.ApiClient.Tests;

public sealed class ClientusClientLifecycleTests
{
    [Fact]
    public void ServiceProperties_AreStableInstances()
    {
        using var client = CreateClient();

        Assert.Same(client.Auth, client.Auth);
        Assert.Same(client.Customers, client.Customers);
        Assert.Same(client.Quotes, client.Quotes);
        Assert.Same(client.Users, client.Users);
    }

    [Fact]
    public async Task RepeatedDisposal_IsSafeAndServicesRejectOperations()
    {
        var client = CreateClient();
        var customers = client.Customers;
        var quotes = client.Quotes;

        client.Dispose();
        client.Dispose();

        Assert.Throws<ObjectDisposedException>(() => client.Auth);
        Assert.Throws<ObjectDisposedException>(() => client.Customers);
        Assert.Throws<ObjectDisposedException>(() => client.Quotes);
        Assert.Throws<ObjectDisposedException>(() => client.Users);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => customers.ExistsAsync("customer"));
        await Assert.ThrowsAsync<ObjectDisposedException>(() => quotes.ExistsAsync("quote"));
    }

    private static ClientusClient CreateClient() => new(new ClientusConfiguration
    {
        BaseUrl = "https://api.example.test",
        ApiKey = "key"
    });
}
