using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Clientus.ApiClient.Authentication;
using Clientus.ApiClient.Authentication.Models;
using Clientus.ApiClient.Common;
using Clientus.ApiClient.Configuration;
using Clientus.ApiClient.Http;
using Clientus.ApiClient.Pagination;

namespace Clientus.ApiClient.Tests;

public sealed class PublicApiTransitionFoundationTests
{
    [Fact]
    public async Task UsernameLogin_FailsClosedWithoutCallingLegacyRpc()
    {
        var handler = new RecordingHandler(_ => throw new InvalidOperationException("No request expected."));
        using var http = CreateClient(handler);
        var auth = new AuthService(http);

        var result = await auth.LoginAsync(new LoginRequest { Identifier = "legacy-user", Password = "secret" });

        Assert.False(result.Success);
        Assert.Contains("email address", result.Error, StringComparison.Ordinal);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task ErrorMapping_ReadsFutureNeutralMetadataWithoutInventingValues()
    {
        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
        {
            Content = new StringContent(
                "{\"code\":\"rate_limited\",\"message\":\"Slow down.\",\"errors\":{\"pageSize\":[\"Too large.\"]}}",
                Encoding.UTF8,
                "application/json")
        };
        response.Headers.Add("x-request-id", "req-123");
        response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(12));
        var handler = new RecordingHandler(_ => response);
        using var http = CreateClient(handler);

        var error = await Assert.ThrowsAsync<ApiException>(() => http.GetAsync<object>("/test"));

        Assert.Equal("rate_limited", error.ErrorCode);
        Assert.Equal("Slow down.", error.Message);
        Assert.Equal("req-123", error.RequestId);
        Assert.True(error.IsRetryable);
        Assert.Equal(TimeSpan.FromSeconds(12), error.RetryAfter);
        Assert.Equal("Too large.", Assert.Single(error.ValidationErrors!["pageSize"]));
    }

    [Fact]
    public async Task Requests_IdentifyTheSdkWithoutSendingTelemetry()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        });
        using var http = CreateClient(handler);

        await http.GetAsync<object>("/test");

        var request = Assert.Single(handler.Requests);
        Assert.StartsWith("Clientus-DotNet-SDK/1.0.0-beta.2", request.Headers.UserAgent.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(request.Headers, header => header.Key.StartsWith("trace", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task RetryAfter_IsHonoredWithinConfiguredBoundForSafeRequests()
    {
        var attempts = 0;
        var handler = new RecordingHandler(_ =>
        {
            attempts++;
            if (attempts == 1)
            {
                var throttled = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
                throttled.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromHours(1));
                return throttled;
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };
        });
        var configuration = Configuration();
        configuration.HttpMessageHandler = handler;
        configuration.MaxRetryAttempts = 2;
        configuration.MaximumRetryDelay = TimeSpan.Zero;
        using var http = new ClientusHttpClient(configuration);

        await http.GetAsync<object>("/test");

        Assert.Equal(2, attempts);
    }

    [Fact]
    public void CustomHandler_CanRemainCallerOwned()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var configuration = Configuration();
        configuration.HttpMessageHandler = handler;
        configuration.DisposeHttpMessageHandler = false;

        using (var http = new ClientusHttpClient(configuration))
        {
        }

        Assert.Equal(0, handler.DisposeCount);
        handler.Dispose();
        Assert.Equal(1, handler.DisposeCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void PageRequest_RejectsUnsupportedPageSizes(int pageSize)
    {
        var request = new PageRequest(pageSize);
        Assert.Throws<ArgumentOutOfRangeException>(request.Validate);
    }

    [Fact]
    public void Page_UsesOpaqueContinuationMetadata()
    {
        var page = new Page<int>(new[] { 1, 2 }, "opaque-token", 20);
        Assert.True(page.HasMore);
        Assert.Equal(20, page.TotalCount);
        Assert.Equal(new[] { 1, 2 }, page.Items);
    }

    private static ClientusHttpClient CreateClient(RecordingHandler handler)
    {
        var configuration = Configuration();
        configuration.HttpMessageHandler = handler;
        return new ClientusHttpClient(configuration);
    }

    private static ClientusConfiguration Configuration() => new()
    {
        BaseUrl = "https://api.example.test",
        ApiKey = "legacy-publishable-key",
        MaxRetryAttempts = 1,
        InitialRetryDelay = TimeSpan.Zero
    };

    private sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> factory) : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];
        public int DisposeCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(factory(request));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeCount++;
            }

            base.Dispose(disposing);
        }
    }
}
