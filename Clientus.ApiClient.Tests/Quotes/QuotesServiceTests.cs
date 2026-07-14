using Clientus.ApiClient.Configuration;
using Clientus.ApiClient.Http;
using Clientus.ApiClient.Quotes;

namespace Clientus.ApiClient.Tests.Quotes;

public sealed class QuotesServiceTests
{
    [Fact]
    public void Constructor_NullHttpClientThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new QuotesService(null!));
    }

    [Fact]
    public void ClientusClient_ExposesSingleQuotesServiceInstance()
    {
        using var client = CreateClient();

        var first = client.Quotes;
        var second = client.Quotes;

        Assert.NotNull(first);
        Assert.Same(first, second);
    }

    [Fact]
    public void ClientusClient_QuotesAfterDisposalThrows()
    {
        var client = CreateClient();
        client.Dispose();

        Assert.Throws<ObjectDisposedException>(() => client.Quotes);
    }

    [Fact]
    public void QuotesService_DetectsDisposedUnderlyingClient()
    {
        using var http = new ClientusHttpClient(CreateConfiguration());
        var service = new QuotesService(http);
        http.Dispose();

        Assert.Throws<ObjectDisposedException>(() => service.ThrowIfDisposed());
    }

    private static ClientusClient CreateClient()
    {
        return new ClientusClient(CreateConfiguration());
    }

    private static ClientusConfiguration CreateConfiguration()
    {
        return new ClientusConfiguration
        {
            BaseUrl = "https://api.example.test",
            ApiKey = "test-key"
        };
    }
}
