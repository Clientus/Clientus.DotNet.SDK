using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Clientus.ApiClient.Configuration;
using Clientus.ApiClient.Home;

namespace Clientus.ApiClient.Tests;

public sealed class HomeReadSurfaceTests
{
    [Fact]
    public void ClientusClient_ExposesHomeReadSurface()
    {
        using var client = CreateClient(new RecordingHandler(_ => Json("{\"data\":{}}")));
        Assert.NotNull(client.Home);
        Assert.IsType<HomeService>(client.Home);
    }

    [Fact]
    public async Task HomeReads_UseExactPublicApiV1RoutesAndBearerAuth()
    {
        var handler = new RecordingHandler(request =>
            request.RequestUri!.AbsolutePath == "/api/v1/home/members"
                ? Json("{\"data\":[],\"pagination\":{\"page\":1,\"pageSize\":25,\"total\":0,\"hasNext\":false},\"requestId\":\"00000000-0000-0000-0000-000000000001\"}")
                : Json("{\"data\":{},\"requestId\":\"00000000-0000-0000-0000-000000000001\"}"));
        using var client = CreateClient(handler);

        await client.Home.GetContextAsync();
        await client.Home.ListMembersAsync();
        await client.Home.GetDocumentsOverviewAsync();

        Assert.Equal(new[]
        {
            "/api/v1/home/context",
            "/api/v1/home/members?page=1&pageSize=25",
            "/api/v1/home/documents/overview"
        }, handler.Requests.Select(r => r.PathAndQuery));

        Assert.All(handler.Requests, request =>
        {
            Assert.Equal("Bearer", request.Authorization?.Scheme);
            Assert.Equal("developer-test-credential", request.Authorization?.Parameter);
            Assert.False(request.Headers.ContainsKey("apikey"));
        });
    }

    [Theory]
    [InlineData(0, 25)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task HomeMemberPaging_RejectsOutOfContractValues(int page, int pageSize)
    {
        using var client = CreateClient(new RecordingHandler(_ => throw new InvalidOperationException("No request expected.")));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.Home.ListMembersAsync(page, pageSize));
    }

    [Fact]
    public void HomeSurface_IsReadOnly()
    {
        var publicMethods = typeof(HomeService).GetMethods()
            .Where(m => m.DeclaringType == typeof(HomeService))
            .Select(m => m.Name)
            .ToArray();

        var forbiddenPrefixes = new[] { "Create", "Update", "Delete", "Post", "Patch", "Put", "Upload", "Share", "Invite" };
        Assert.DoesNotContain(publicMethods, name =>
            forbiddenPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal)));
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
        public List<Snapshot> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(new Snapshot(
                request.RequestUri!.PathAndQuery,
                request.Headers.Authorization,
                request.Headers.ToDictionary(x => x.Key, x => x.Value.ToArray(), StringComparer.OrdinalIgnoreCase)));
            return Task.FromResult(factory(request));
        }
    }
}
