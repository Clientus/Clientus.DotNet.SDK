using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Clientus.ApiClient.Authentication;
using Clientus.ApiClient.Authentication.Models;
using Clientus.ApiClient.Common;
using Clientus.ApiClient.Configuration;
using Clientus.ApiClient.Http;

namespace Clientus.ApiClient.Tests.Http;

public sealed class ClientusHttpClientTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("relative/path")]
    [InlineData("ftp://example.test")]
    public void Constructor_RejectsInvalidBaseUrl(string? baseUrl)
    {
        var configuration = ValidConfiguration();
        configuration.BaseUrl = baseUrl!;
        Assert.Throws<ArgumentException>(() => new ClientusHttpClient(configuration));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsMissingApiKey(string? apiKey)
    {
        var configuration = ValidConfiguration();
        configuration.ApiKey = apiKey!;
        Assert.Throws<ArgumentException>(() => new ClientusHttpClient(configuration));
    }

    [Fact]
    public void Constructor_ValidatesTimeoutAndRetrySettings()
    {
        var timeout = ValidConfiguration();
        timeout.Timeout = TimeSpan.Zero;
        Assert.Throws<ArgumentOutOfRangeException>(() => new ClientusHttpClient(timeout));

        var attempts = ValidConfiguration();
        attempts.MaxRetryAttempts = 0;
        Assert.Throws<ArgumentOutOfRangeException>(() => new ClientusHttpClient(attempts));

        var delay = ValidConfiguration();
        delay.InitialRetryDelay = TimeSpan.FromMilliseconds(-1);
        Assert.Throws<ArgumentOutOfRangeException>(() => new ClientusHttpClient(delay));

        var infinite = ValidConfiguration();
        infinite.Timeout = Timeout.InfiniteTimeSpan;
        using var client = new ClientusHttpClient(infinite);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("HEAD")]
    [InlineData("DELETE")]
    public async Task IdempotentOperations_RetryTransientResponsesWithFreshRequests(string method)
    {
        var handler = new RecordingHandler(attempt => attempt < 3
            ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            : Success(method));
        using var client = CreateClient(handler, 3);

        await InvokeAsync(client, method);

        Assert.Equal(3, handler.Requests.Count);
        Assert.Equal(3, handler.Requests.Distinct(ReferenceEqualityComparer.Instance).Count());
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PATCH")]
    public async Task PostAndPatch_DoNotRetryAndKeepBodiesValid(string method)
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        using var client = CreateClient(handler, 3);

        await Assert.ThrowsAsync<ApiException>(() => InvokeAsync(client, method));

        Assert.Single(handler.Requests);
        Assert.Equal("{\"value\":\"body\"}", Assert.Single(handler.Bodies));
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("HEAD")]
    [InlineData("DELETE")]
    public async Task NonTransientFailures_AreNotRetriedAndPreserveError(string method)
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent("denied")
        });
        using var client = CreateClient(handler, 3);

        var error = await Assert.ThrowsAsync<ApiException>(() => InvokeAsync(client, method));

        Assert.Single(handler.Requests);
        Assert.Equal(HttpStatusCode.Forbidden, error.StatusCode);
        Assert.Equal("denied", error.ResponseBody);
        Assert.Contains("403", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("HEAD")]
    [InlineData("DELETE")]
    [InlineData("POST")]
    [InlineData("PATCH")]
    public async Task Cancellation_IsNeverRetried(string method)
    {
        var handler = new RecordingHandler((_, token) => throw new OperationCanceledException(token));
        using var client = CreateClient(handler, 3);
        using var source = new CancellationTokenSource();
        source.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => InvokeAsync(client, method, source.Token));
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task RetriedAndFinalResponses_AreDisposed()
    {
        var contents = new List<TrackingContent>();
        var handler = new RecordingHandler(attempt =>
        {
            var content = new TrackingContent(attempt == 1 ? "retry" : "[]");
            contents.Add(content);
            return new HttpResponseMessage(attempt == 1 ? HttpStatusCode.BadGateway : HttpStatusCode.OK)
            {
                Content = content
            };
        });
        using var client = CreateClient(handler, 2);

        await client.GetAsync<List<object>>("/rest/v1/test");

        Assert.All(contents, content => Assert.True(content.IsDisposed));
    }

    [Fact]
    public void Dispose_IsIdempotentAndDisposesOwnedHandlerOnce()
    {
        var handler = new RecordingHandler(_ => Success("GET"));
        var client = CreateClient(handler);
        client.Dispose();
        client.Dispose();
        Assert.Equal(1, handler.DisposeCount);
        Assert.Throws<ObjectDisposedException>(() => client.SetAccessToken("token"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Operations_RejectMissingEndpoints(string? endpoint)
    {
        var handler = new RecordingHandler(_ => Success("GET"));
        using var client = CreateClient(handler);
        await Assert.ThrowsAsync<ArgumentException>(() => client.GetAsync<object>(endpoint!));
        await Assert.ThrowsAsync<ArgumentException>(() => client.PostAsync<object>(endpoint!, new { }));
        await Assert.ThrowsAsync<ArgumentException>(() => client.PatchAsync<object>(endpoint!, new { }));
        await Assert.ThrowsAsync<ArgumentException>(() => client.DeleteAsync(endpoint!));
        await Assert.ThrowsAsync<ArgumentException>(() => client.HeadCountAsync(endpoint!));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task PostAndPatch_RejectNullBodiesWithoutRequests()
    {
        var handler = new RecordingHandler(_ => Success("GET"));
        using var client = CreateClient(handler);
        await Assert.ThrowsAsync<ArgumentNullException>(() => client.PostAsync<object>("/test", null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => client.PatchAsync<object>("/test", null!));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task Logout_PropagatesCallerCancellationAndKeepsSession()
    {
        var handler = new RecordingHandler((attempt, token) => attempt == 1
            ? new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"access_token\":\"access\",\"refresh_token\":\"refresh\"}",
                    Encoding.UTF8,
                    "application/json")
            }
            : throw new OperationCanceledException(token));
        using var client = CreateClient(handler);
        var auth = new AuthService(client);
        var login = await auth.LoginAsync(new LoginRequest
        {
            Identifier = "user@example.test",
            Password = "password"
        });
        Assert.True(login.Success);
        using var source = new CancellationTokenSource();
        source.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => auth.LogoutAsync(source.Token));

        Assert.True(auth.IsAuthenticated);
        Assert.Equal(2, handler.Requests.Count);
    }

    private static Task InvokeAsync(ClientusHttpClient client, string method, CancellationToken token = default) => method switch
    {
        "GET" => client.GetAsync<List<object>>("/rest/v1/test", token),
        "HEAD" => client.HeadCountAsync("/rest/v1/test?select=id", token),
        "DELETE" => client.DeleteAsync("/rest/v1/test?id=eq.1", token),
        "POST" => client.PostAsync<object>("/rest/v1/test", new { value = "body" }, token),
        "PATCH" => client.PatchAsync<object>("/rest/v1/test?id=eq.1", new { value = "body" }, token),
        _ => throw new ArgumentOutOfRangeException(nameof(method))
    };

    private static HttpResponseMessage Success(string method)
    {
        var response = new HttpResponseMessage(method == "HEAD" ? HttpStatusCode.PartialContent : HttpStatusCode.OK)
        {
            Content = new StringContent(method is "GET" or "POST" or "PATCH" ? "[]" : string.Empty, Encoding.UTF8, "application/json")
        };
        if (method == "HEAD") response.Content.Headers.ContentRange = new ContentRangeHeaderValue(0, 0, 0);
        return response;
    }

    private static ClientusHttpClient CreateClient(RecordingHandler handler, int attempts = 1) =>
        new(ValidConfiguration(attempts), handler);

    private static ClientusConfiguration ValidConfiguration(int attempts = 1) => new()
    {
        BaseUrl = "https://api.example.test",
        ApiKey = "key",
        MaxRetryAttempts = attempts,
        InitialRetryDelay = TimeSpan.Zero
    };

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly Func<int, CancellationToken, HttpResponseMessage> _factory;
        public RecordingHandler(Func<int, HttpResponseMessage> factory) : this((attempt, _) => factory(attempt)) { }
        public RecordingHandler(Func<int, CancellationToken, HttpResponseMessage> factory) => _factory = factory;
        public List<HttpRequestMessage> Requests { get; } = [];
        public List<string> Bodies { get; } = [];
        public int DisposeCount { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            if (request.Content is not null) Bodies.Add(await request.Content.ReadAsStringAsync(cancellationToken));
            return _factory(Requests.Count, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) DisposeCount++;
            base.Dispose(disposing);
        }
    }

    private sealed class TrackingContent(string value) : StringContent(value, Encoding.UTF8, "application/json")
    {
        public bool IsDisposed { get; private set; }
        protected override void Dispose(bool disposing)
        {
            if (disposing) IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}
