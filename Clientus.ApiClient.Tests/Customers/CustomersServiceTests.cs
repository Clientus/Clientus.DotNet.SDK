using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Clientus.ApiClient.Common;
using Clientus.ApiClient.Configuration;
using Clientus.ApiClient.Customers;
using Clientus.ApiClient.Http;

namespace Clientus.ApiClient.Tests.Customers;

public sealed class CustomersServiceTests
{
    [Fact]
    public async Task GetAllAsync_SendsExpectedRequestAndReturnsCustomers()
    {
        using var fixture = CreateFixture(JsonResponse("[{\"id\":\"customer-1\",\"first_name\":\"Ada\"}]"));

        var customers = await fixture.Service.GetAllAsync(10);

        var customer = Assert.Single(customers);
        Assert.Equal("customer-1", customer.Id);
        Assert.Equal("Ada", customer.FirstName);
        var request = Assert.Single(fixture.Handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Contains("select=id,company_id,first_name", request.Uri.Query, StringComparison.Ordinal);
        Assert.Contains("order=created_at.desc", request.Uri.Query, StringComparison.Ordinal);
        Assert.Contains("limit=10", request.Uri.Query, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetAllAsync_NullResponseReturnsEmptyList()
    {
        using var fixture = CreateFixture(JsonResponse("null"));

        var customers = await fixture.Service.GetAllAsync();

        Assert.Empty(customers);
        Assert.DoesNotContain("limit=", Assert.Single(fixture.Handler.Requests).Uri.Query, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetAllAsync_NonPositiveLimitThrows(int limit)
    {
        using var fixture = CreateFixture(JsonResponse("[]"));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => fixture.Service.GetAllAsync(limit));

        Assert.Empty(fixture.Handler.Requests);
    }

    [Fact]
    public async Task GetByIdAsync_EscapesIdentifierAndReturnsFirstMatch()
    {
        using var fixture = CreateFixture(JsonResponse("[{\"id\":\"a/b c\"}]"));

        var customer = await fixture.Service.GetByIdAsync("a/b c");

        Assert.Equal("a/b c", customer?.Id);
        var request = Assert.Single(fixture.Handler.Requests);
        Assert.Contains("id=eq.a%2Fb%20c", request.Uri.OriginalString, StringComparison.Ordinal);
        Assert.Contains("limit=1", request.Uri.Query, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetByIdAsync_MissingIdentifierThrows(string? id)
    {
        using var fixture = CreateFixture(JsonResponse("[]"));

        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.GetByIdAsync(id!));

        Assert.Empty(fixture.Handler.Requests);
    }

    [Fact]
    public async Task SearchAsync_WhitespaceReturnsEmptyWithoutRequest()
    {
        using var fixture = CreateFixture(JsonResponse("[]"));

        var customers = await fixture.Service.SearchAsync("  ");

        Assert.Empty(customers);
        Assert.Empty(fixture.Handler.Requests);
    }

    [Fact]
    public async Task SearchAsync_SendsEscapedSearchAndLimit()
    {
        using var fixture = CreateFixture(JsonResponse("[]"));

        await fixture.Service.SearchAsync(" Ada & Co ", 7);

        var request = Assert.Single(fixture.Handler.Requests);
        Assert.Contains("%2AAda%20%26%20Co%2A", request.Uri.OriginalString, StringComparison.Ordinal);
        Assert.Contains("limit=7", request.Uri.Query, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SearchAsync_NonPositiveLimitThrows()
    {
        using var fixture = CreateFixture(JsonResponse("[]"));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => fixture.Service.SearchAsync("Ada", 0));

        Assert.Empty(fixture.Handler.Requests);
    }

    [Fact]
    public async Task UpdateAsync_UsesPatchReturnsEntityAndExcludesProtectedFields()
    {
        var createdAt = DateTimeOffset.Parse("2026-01-02T03:04:05Z");
        var input = new Customer
        {
            Id = "customer/1",
            CompanyId = "caller-controlled-company",
            FirstName = "Ada",
            LastName = "Lovelace",
            DisplayName = "Ada L.",
            Email = "ada@example.test",
            Phone = "123",
            Address = "Main Street",
            PostalCode = "8000",
            City = "Zurich",
            Country = "CH",
            ClientType = "person",
            CreatedAt = createdAt
        };
        using var fixture = CreateFixture(JsonResponse("[{\"id\":\"customer/1\",\"company_id\":\"server-company\",\"first_name\":\"Ada\"}]"));

        var updated = await fixture.Service.UpdateAsync(input);

        Assert.Equal("customer/1", updated.Id);
        Assert.Equal("server-company", updated.CompanyId);
        var request = Assert.Single(fixture.Handler.Requests);
        Assert.Equal(HttpMethod.Patch, request.Method);
        Assert.Contains("id=eq.customer%2F1", request.Uri.OriginalString, StringComparison.Ordinal);
        Assert.Equal("return=representation", Assert.Single(request.Headers["Prefer"]));
        using var payload = JsonDocument.Parse(request.Body!);
        Assert.False(payload.RootElement.TryGetProperty("id", out _));
        Assert.False(payload.RootElement.TryGetProperty("company_id", out _));
        Assert.False(payload.RootElement.TryGetProperty("created_at", out _));
        Assert.Equal("Ada", payload.RootElement.GetProperty("first_name").GetString());
        Assert.Equal("caller-controlled-company", input.CompanyId);
        Assert.Equal(createdAt, input.CreatedAt);
    }

    [Fact]
    public async Task UpdateAsync_NullCustomerThrows()
    {
        using var fixture = CreateFixture(JsonResponse("[]"));

        await Assert.ThrowsAsync<ArgumentNullException>(() => fixture.Service.UpdateAsync(null!));

        Assert.Empty(fixture.Handler.Requests);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateAsync_MissingIdentifierThrows(string id)
    {
        using var fixture = CreateFixture(JsonResponse("[]"));

        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.UpdateAsync(new Customer { Id = id }));

        Assert.Empty(fixture.Handler.Requests);
    }

    [Fact]
    public async Task UpdateAsync_EmptyRepresentationThrows()
    {
        using var fixture = CreateFixture(JsonResponse("[]"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => fixture.Service.UpdateAsync(new Customer { Id = "missing" }));
    }

    [Fact]
    public async Task UpdateAsync_DoesNotRetryTransientFailure()
    {
        using var fixture = CreateFixture(
            _ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            maxRetryAttempts: 3);

        await Assert.ThrowsAsync<ApiException>(
            () => fixture.Service.UpdateAsync(new Customer { Id = "customer-1" }));

        Assert.Single(fixture.Handler.Requests);
    }

    [Fact]
    public async Task DeleteAsync_TargetsOnlySpecifiedCustomer()
    {
        using var fixture = CreateFixture(new HttpResponseMessage(HttpStatusCode.NoContent));

        await fixture.Service.DeleteAsync("customer/1");

        var request = Assert.Single(fixture.Handler.Requests);
        Assert.Equal(HttpMethod.Delete, request.Method);
        Assert.Equal("?id=eq.customer%2F1", request.Uri.Query);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task DeleteAsync_MissingIdentifierThrows(string? id)
    {
        using var fixture = CreateFixture(new HttpResponseMessage(HttpStatusCode.NoContent));

        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.DeleteAsync(id!));

        Assert.Empty(fixture.Handler.Requests);
    }

    [Fact]
    public async Task DeleteAsync_RetriesTransientFailure()
    {
        using var fixture = CreateFixture(
            attempt => new HttpResponseMessage(
                attempt == 1 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.NoContent),
            maxRetryAttempts: 3);

        await fixture.Service.DeleteAsync("customer-1");

        Assert.Equal(2, fixture.Handler.Requests.Count);
    }

    [Theory]
    [InlineData("[{\"id\":\"customer-1\"}]", true)]
    [InlineData("[]", false)]
    [InlineData("null", false)]
    public async Task ExistsAsync_ReturnsWhetherLightweightQueryFindsCustomer(string json, bool expected)
    {
        using var fixture = CreateFixture(JsonResponse(json));

        var exists = await fixture.Service.ExistsAsync("customer-1");

        Assert.Equal(expected, exists);
        var request = Assert.Single(fixture.Handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Contains("select=id", request.Uri.Query, StringComparison.Ordinal);
        Assert.Contains("limit=1", request.Uri.Query, StringComparison.Ordinal);
        Assert.DoesNotContain("company_id", request.Uri.Query, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExistsAsync_MissingIdentifierThrows(string? id)
    {
        using var fixture = CreateFixture(JsonResponse("[]"));

        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.ExistsAsync(id!));

        Assert.Empty(fixture.Handler.Requests);
    }

    [Fact]
    public async Task CountAsync_UsesHeadExactCountAndRange()
    {
        var response = new HttpResponseMessage(HttpStatusCode.PartialContent);
        response.Content.Headers.ContentRange = new ContentRangeHeaderValue(0, 0, 42);
        using var fixture = CreateFixture(response);

        var count = await fixture.Service.CountAsync();

        Assert.Equal(42, count);
        var request = Assert.Single(fixture.Handler.Requests);
        Assert.Equal(HttpMethod.Head, request.Method);
        Assert.Equal("count=exact", Assert.Single(request.Headers["Prefer"]));
        Assert.Equal("bytes=0-0", request.Headers["Range"].Single());
        Assert.Equal("?select=id", request.Uri.Query);
    }

    [Fact]
    public async Task CountAsync_MissingContentRangeThrows()
    {
        using var fixture = CreateFixture(new HttpResponseMessage(HttpStatusCode.OK));

        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Service.CountAsync());
    }

    [Fact]
    public async Task CountAsync_RetriesTransientFailure()
    {
        using var fixture = CreateFixture(
            attempt =>
            {
                var response = new HttpResponseMessage(
                    attempt == 1 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.PartialContent);
                if (attempt > 1)
                {
                    response.Content.Headers.ContentRange = new ContentRangeHeaderValue(0, 0, 3);
                }

                return response;
            },
            maxRetryAttempts: 3);

        var count = await fixture.Service.CountAsync();

        Assert.Equal(3, count);
        Assert.Equal(2, fixture.Handler.Requests.Count);
    }

    [Fact]
    public async Task Operations_PropagateApiErrors()
    {
        using var fixture = CreateFixture(
            new HttpResponseMessage(HttpStatusCode.Forbidden)
            {
                Content = new StringContent("denied")
            });

        var exception = await Assert.ThrowsAsync<ApiException>(
            () => fixture.Service.ExistsAsync("customer-1"));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Equal("denied", exception.ResponseBody);
    }

    [Fact]
    public async Task Operations_PropagateCancellation()
    {
        using var fixture = CreateFixture(
            (_, _, cancellationToken) => throw new OperationCanceledException(cancellationToken));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => fixture.Service.ExistsAsync("customer-1", cancellation.Token));
    }

    [Fact]
    public async Task Operations_AfterClientDisposalThrow()
    {
        var fixture = CreateFixture(JsonResponse("[]"));
        fixture.Client.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(
            () => fixture.Service.ExistsAsync("customer-1"));

        fixture.Dispose();
    }

    private static Fixture CreateFixture(
        HttpResponseMessage response,
        int maxRetryAttempts = 1)
    {
        return CreateFixture(_ => response, maxRetryAttempts);
    }

    private static Fixture CreateFixture(
        Func<int, HttpResponseMessage> responseFactory,
        int maxRetryAttempts = 1)
    {
        return CreateFixture((attempt, _, _) => responseFactory(attempt), maxRetryAttempts);
    }

    private static Fixture CreateFixture(
        Func<int, HttpRequestMessage, CancellationToken, HttpResponseMessage> responseFactory,
        int maxRetryAttempts = 1)
    {
        var handler = new RecordingHandler(responseFactory);
        var client = new ClientusHttpClient(
            new ClientusConfiguration
            {
                BaseUrl = "https://api.example.test",
                ApiKey = "test-key",
                MaxRetryAttempts = maxRetryAttempts,
                InitialRetryDelay = TimeSpan.Zero
            },
            handler);

        return new Fixture(client, new CustomersService(client), handler);
    }

    private static HttpResponseMessage JsonResponse(string json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    private sealed record RequestSnapshot(
        HttpMethod Method,
        Uri Uri,
        IReadOnlyDictionary<string, string[]> Headers,
        string? Body);

    private sealed class RecordingHandler(
        Func<int, HttpRequestMessage, CancellationToken, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        public List<RequestSnapshot> Requests { get; } = new();

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var headers = request.Headers
                .Concat(request.Content?.Headers ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>())
                .ToDictionary(pair => pair.Key, pair => pair.Value.ToArray(), StringComparer.OrdinalIgnoreCase);
            var body = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            var uri = request.RequestUri
                ?? throw new InvalidOperationException("The request URI was not set.");

            Requests.Add(new RequestSnapshot(request.Method, uri, headers, body));
            return responseFactory(Requests.Count, request, cancellationToken);
        }
    }

    private sealed class Fixture(
        ClientusHttpClient client,
        CustomersService service,
        RecordingHandler handler) : IDisposable
    {
        public ClientusHttpClient Client { get; } = client;

        public CustomersService Service { get; } = service;

        public RecordingHandler Handler { get; } = handler;

        public void Dispose()
        {
            Client.Dispose();
        }
    }
}
