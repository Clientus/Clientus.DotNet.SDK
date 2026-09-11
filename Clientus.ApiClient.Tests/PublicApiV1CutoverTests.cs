using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Clientus.ApiClient;
using Clientus.ApiClient.Configuration;
using Clientus.ApiClient.Http;

namespace Clientus.ApiClient.Tests;

public sealed class PublicApiV1CutoverTests
{
    [Fact]
    public async Task DeveloperCredential_IsSentAsBearer_WithoutSupabaseApiKey()
    {
        var handler = new RecordingHandler(_ => Json("{\"data\":[]}"));
        using var client = new ClientusClient(Config(handler));

        await client.Customers.GetAllAsync(limit: 25);

        var request = Assert.Single(handler.Requests);
        Assert.Equal("Bearer", request.Authorization?.Scheme);
        Assert.Equal("developer-test-credential", request.Authorization?.Parameter);
        Assert.False(request.Headers.ContainsKey("apikey"));
        Assert.Equal("/api/v1/customers?limit=25", request.PathAndQuery);
    }

    [Fact]
    public async Task PublicApi_DataEnvelope_IsUnwrapped()
    {
        var handler = new RecordingHandler(_ => Json("{\"data\":[{\"id\":\"c1\",\"display_name\":\"Ada\"}]}"));
        using var client = new ClientusClient(Config(handler));

        var customers = await client.Customers.GetAllAsync();

        Assert.Single(customers);
        Assert.Equal("c1", customers[0].Id);
    }

    [Theory]
    [InlineData("customers", "/api/v1/customers/id%2F1")]
    [InlineData("catalog", "/api/v1/catalog/id%2F1")]
    [InlineData("quotes", "/api/v1/quotes/id%2F1")]
    [InlineData("invoices", "/api/v1/invoices/id%2F1")]
    public async Task ExactReads_UseVersionedRoutes(string service, string expected)
    {
        var handler = new RecordingHandler(_ => Json("{\"data\":null}"));
        using var client = new ClientusClient(Config(handler));

        switch (service)
        {
            case "customers": await client.Customers.GetByIdAsync("id/1"); break;
            case "catalog": await client.Catalog.GetAsync("id/1"); break;
            case "quotes": await client.Quotes.GetAsync("id/1"); break;
            case "invoices": await client.Invoices.GetAsync("id/1"); break;
        }

        Assert.Equal(expected, Assert.Single(handler.Requests).PathAndQuery);
    }

    [Fact]
    public async Task Me_UsesVersionedCurrentContextRoute()
    {
        var handler = new RecordingHandler(_ => Json("{\"data\":null}"));
        using var client = new ClientusClient(Config(handler));

        await client.Users.GetCurrentAsync();

        Assert.Equal("/api/v1/me", Assert.Single(handler.Requests).PathAndQuery);
    }

    [Fact]
    public async Task LegacyHumanAuth_FailsClosedWithoutNetwork()
    {
        var handler = new RecordingHandler(_ => throw new InvalidOperationException("No request expected."));
        using var client = new ClientusClient(Config(handler));

#pragma warning disable CS0618
        var result = await client.Auth.LoginAsync(new()
        {
            Identifier = "user@example.test",
            Password = "password"
        });
#pragma warning restore CS0618

        Assert.False(result.Success);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task UnsupportedMutations_FailClosedWithoutNetwork()
    {
        var handler = new RecordingHandler(_ => throw new InvalidOperationException("No request expected."));
        using var client = new ClientusClient(Config(handler));

#pragma warning disable CS0618
        await Assert.ThrowsAsync<NotSupportedException>(() => client.Customers.DeleteAsync("c1"));
        await Assert.ThrowsAsync<NotSupportedException>(() => client.Catalog.DeleteAsync("p1"));
        await Assert.ThrowsAsync<NotSupportedException>(() => client.Quotes.DeleteAsync("q1"));
        await Assert.ThrowsAsync<NotSupportedException>(() => client.Invoices.DeleteAsync("i1"));
#pragma warning restore CS0618

        Assert.Empty(handler.Requests);
    }

    [Fact]
    public void MissingDeveloperCredential_IsRejected()
    {
        var config = Config(new RecordingHandler(_ => Json("{}")));
        config.DeveloperCredential = "";
        Assert.Throws<ArgumentException>(() => new ClientusHttpClient(config));
    }

    private static ClientusConfiguration Config(HttpMessageHandler handler) => new()
    {
        BaseUrl = "https://api.example.test",
        DeveloperCredential = "developer-test-credential",
        Environment = ClientusEnvironment.Sandbox,
        HttpMessageHandler = handler,
        MaxRetryAttempts = 1,
        InitialRetryDelay = TimeSpan.Zero
    };

    private static HttpResponseMessage Json(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    private sealed record Snapshot(
        string PathAndQuery,
        AuthenticationHeaderValue? Authorization,
        IReadOnlyDictionary<string, string[]> Headers);

    private sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> factory) : HttpMessageHandler
    {
        public List<Snapshot> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(new Snapshot(
                request.RequestUri!.PathAndQuery,
                request.Headers.Authorization,
                request.Headers.ToDictionary(x => x.Key, x => x.Value.ToArray(), StringComparer.OrdinalIgnoreCase)));
            return Task.FromResult(factory(request));
        }
    }
}