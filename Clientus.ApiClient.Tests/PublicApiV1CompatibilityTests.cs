using System.Net;
using System.Text;
using Clientus.ApiClient;
using Clientus.ApiClient.Configuration;

namespace Clientus.ApiClient.Tests;

public sealed class PublicApiV1CompatibilityTests
{
    [Fact]
    public async Task CustomerSearch_FailsClosedWithoutNetwork()
    {
        using var client = CreateClient(new NoNetworkHandler());
#pragma warning disable CS0618
        await Assert.ThrowsAsync<NotSupportedException>(
            () => client.Customers.SearchAsync("Ada"));
#pragma warning restore CS0618
    }

    [Fact]
    public async Task CustomerUpdateDeleteCountExist_FailClosedWithoutNetwork()
    {
        using var client = CreateClient(new NoNetworkHandler());
#pragma warning disable CS0618
        await Assert.ThrowsAsync<NotSupportedException>(
            () => client.Customers.UpdateAsync(new()));
        await Assert.ThrowsAsync<NotSupportedException>(
            () => client.Customers.DeleteAsync("c1"));
        await Assert.ThrowsAsync<NotSupportedException>(
            () => client.Customers.ExistsAsync("c1"));
        await Assert.ThrowsAsync<NotSupportedException>(
            () => client.Customers.CountAsync());
#pragma warning restore CS0618
    }

    [Fact]
    public async Task CatalogLegacyOperations_FailClosedWithoutNetwork()
    {
        using var client = CreateClient(new NoNetworkHandler());
#pragma warning disable CS0618
        await Assert.ThrowsAsync<NotSupportedException>(
            () => client.Catalog.SearchAsync("term"));
        await Assert.ThrowsAsync<NotSupportedException>(
            () => client.Catalog.ListCategoriesAsync());
        await Assert.ThrowsAsync<NotSupportedException>(
            () => client.Catalog.CountAsync());
        await Assert.ThrowsAsync<NotSupportedException>(
            () => client.Catalog.DeleteAsync("p1"));
#pragma warning restore CS0618
    }

    [Fact]
    public async Task QuoteLegacyOperations_FailClosedWithoutNetwork()
    {
        using var client = CreateClient(new NoNetworkHandler());
#pragma warning disable CS0618
        await Assert.ThrowsAsync<NotSupportedException>(
            () => client.Quotes.GetWithItemsAsync("q1"));
        await Assert.ThrowsAsync<NotSupportedException>(
            () => client.Quotes.ExistsAsync("q1"));
        await Assert.ThrowsAsync<NotSupportedException>(
            () => client.Quotes.CountAsync());
        await Assert.ThrowsAsync<NotSupportedException>(
            () => client.Quotes.DeleteAsync("q1"));
#pragma warning restore CS0618
    }

    [Fact]
    public async Task InvoiceLegacyOperations_FailClosedWithoutNetwork()
    {
        using var client = CreateClient(new NoNetworkHandler());
#pragma warning disable CS0618
        await Assert.ThrowsAsync<NotSupportedException>(
            () => client.Invoices.GetWithItemsAsync("i1"));
        await Assert.ThrowsAsync<NotSupportedException>(
            () => client.Invoices.ExistsAsync("i1"));
        await Assert.ThrowsAsync<NotSupportedException>(
            () => client.Invoices.CountAsync());
        await Assert.ThrowsAsync<NotSupportedException>(
            () => client.Invoices.DeleteAsync("i1"));
#pragma warning restore CS0618
    }

    private static ClientusClient CreateClient(HttpMessageHandler handler) =>
        new(new ClientusConfiguration
        {
            BaseUrl = "https://api.example.test",
            DeveloperCredential = "developer-test-credential",
            Environment = ClientusEnvironment.Sandbox,
            HttpMessageHandler = handler,
            MaxRetryAttempts = 1,
            InitialRetryDelay = TimeSpan.Zero
        });

    private sealed class NoNetworkHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            throw new InvalidOperationException($"Unexpected network request: {request.RequestUri}");
    }
}