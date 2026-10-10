using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Clientus.ApiClient.Configuration;

namespace Clientus.ApiClient.Tests;

public sealed class Beta5ReadSurfaceTests
{
    [Fact]
    public void ClientusClient_ExposesEveryFrozenBeta5Surface()
    {
        using var client = CreateClient(new RecordingHandler(_ => Json("{\"data\":[]}")));
        Assert.NotNull(client.BusinessUnits);
        Assert.NotNull(client.Warehouse);
        Assert.NotNull(client.Loyalty);
        Assert.NotNull(client.CustomerIntelligence);
        Assert.NotNull(client.RetailReturns);
        Assert.NotNull(client.RetailDistribution);
        Assert.NotNull(client.RetailIntelligence);
        Assert.NotNull(client.Publishing);
        Assert.NotNull(client.Website);
        Assert.NotNull(client.PublicCommerce);
    }

    [Fact]
    public async Task RepresentativeReads_UseExactPublicApiV1RoutesAndBearerAuth()
    {
        var handler = new RecordingHandler(_ => Json("{\"data\":[]}"));
        using var client = CreateClient(handler);

        await client.BusinessUnits.ListAsync();
        await client.Warehouse.ListStockAsync();
        await client.Loyalty.ListProgramsAsync();
        await client.CustomerIntelligence.ListProfilesAsync();
        await client.RetailReturns.ListAsync();
        await client.RetailDistribution.ListChannelsAsync();
        await client.RetailIntelligence.ListProductPerformanceAsync();
        await client.Publishing.ListResourcesAsync();
        await client.Website.ListSitesAsync();

        handler.Factory = _ => Json("{\"data\":{}}");
        await client.PublicCommerce.GetSurfaceAsync();

        var expected = new[]
        {
            "/api/v1/business-units?page=1&pageSize=25",
            "/api/v1/warehouse/stock?page=1&pageSize=25",
            "/api/v1/loyalty/programs?page=1&pageSize=25",
            "/api/v1/customer-intelligence/profiles?page=1&pageSize=25",
            "/api/v1/retail-returns?page=1&pageSize=25",
            "/api/v1/retail-distribution/channels?page=1&pageSize=25",
            "/api/v1/retail-intelligence/product-performance?page=1&pageSize=25",
            "/api/v1/publishing/resources?page=1&pageSize=25",
            "/api/v1/website/sites?page=1&pageSize=25",
            "/api/v1/public-commerce/surface"
        };

        Assert.Equal(expected, handler.Requests.Select(r => r.PathAndQuery));
        Assert.All(handler.Requests, request =>
        {
            Assert.Equal("Bearer", request.Authorization?.Scheme);
            Assert.Equal("developer-test-credential", request.Authorization?.Parameter);
            Assert.False(request.Headers.ContainsKey("apikey"));
        });
    }

    [Fact]
    public async Task DetailIds_AreEscaped()
    {
        var handler = new RecordingHandler(_ => Json("{\"data\":{}}"));
        using var client = CreateClient(handler);

        await client.BusinessUnits.GetAsync("id/1");
        await client.RetailReturns.GetAsync("id/2");
        await client.Loyalty.GetMemberAsync("id/3");
        await client.Loyalty.GetBalanceAsync("id/4");

        Assert.Equal("/api/v1/business-units/id%2F1", handler.Requests[0].PathAndQuery);
        Assert.Equal("/api/v1/retail-returns/id%2F2", handler.Requests[1].PathAndQuery);
        Assert.Equal("/api/v1/loyalty/members/id%2F3", handler.Requests[2].PathAndQuery);
        Assert.Equal("/api/v1/loyalty/balances/id%2F4", handler.Requests[3].PathAndQuery);
    }

    [Theory]
    [InlineData(0, 25)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task Paging_RejectsOutOfContractValues(int page, int pageSize)
    {
        using var client = CreateClient(new RecordingHandler(_ => throw new InvalidOperationException("No request expected.")));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.BusinessUnits.ListAsync(page, pageSize));
    }

    [Fact]
    public void FrozenBeta5Services_DoNotExposeMutationMethods()
    {
        var serviceTypes = new[]
        {
            typeof(Clientus.ApiClient.BusinessUnits.BusinessUnitsService),
            typeof(Clientus.ApiClient.Warehouse.WarehouseService),
            typeof(Clientus.ApiClient.Loyalty.LoyaltyService),
            typeof(Clientus.ApiClient.CustomerIntelligence.CustomerIntelligenceService),
            typeof(Clientus.ApiClient.RetailReturns.RetailReturnsService),
            typeof(Clientus.ApiClient.RetailDistribution.RetailDistributionService),
            typeof(Clientus.ApiClient.RetailIntelligence.RetailIntelligenceService),
            typeof(Clientus.ApiClient.Publishing.PublishingService),
            typeof(Clientus.ApiClient.Website.WebsiteService),
            typeof(Clientus.ApiClient.PublicCommerce.PublicCommerceService)
        };

        var forbiddenPrefixes = new[] { "Create", "Update", "Delete", "Post", "Patch", "Put" };
        foreach (var type in serviceTypes)
        {
            var publicMethods = type.GetMethods()
                .Where(m => m.DeclaringType == type)
                .Select(m => m.Name)
                .ToArray();

            Assert.DoesNotContain(publicMethods, name =>
                forbiddenPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal)));
        }
    }

    private static ClientusClient CreateClient(RecordingHandler handler) =>
        new(new ClientusConfiguration
        {
            BaseUrl = "https://api.example.test",
            DeveloperCredential = "developer-test-credential",
            Environment = ClientusEnvironment.Sandbox,
            HttpMessageHandler = handler,
            MaxRetryAttempts = 1,
            InitialRetryDelay = TimeSpan.Zero
        });

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
        public Func<HttpRequestMessage, HttpResponseMessage> Factory { get; set; } = factory;
        public List<Snapshot> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(new Snapshot(
                request.RequestUri!.PathAndQuery,
                request.Headers.Authorization,
                request.Headers.ToDictionary(x => x.Key, x => x.Value.ToArray(), StringComparer.OrdinalIgnoreCase)));
            return Task.FromResult(Factory(request));
        }
    }
}
