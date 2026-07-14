using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Clientus.ApiClient.Common;
using Clientus.ApiClient.Configuration;
using Clientus.ApiClient.Http;
using Clientus.ApiClient.Quotes;
using Clientus.ApiClient.Serialization;

namespace Clientus.ApiClient.Tests.Quotes;

public sealed class QuotesServiceTests
{
    [Fact]
    public void Models_DeserializeVerifiedFieldsAndEnums()
    {
        const string json = """
        {"id":"q1","company_id":"c1","client_id":null,"created_by":"u1","number":"Q-1",
        "title":"Work","notes":null,"status":"sent","currency":"CHF","subtotal":12.34,
        "tax_rate":8.1,"tax_amount":1.00,"total":13.34,"valid_until":"2026-08-01",
        "issued_at":"2026-07-15","sent_at":"2026-07-15T10:00:00Z","accepted_at":null,
        "rejected_at":null,"client_snapshot":{"display_name":"Ada"},"public_token":"p1",
        "client_response_note":null,"discount_kind":"percent","discount_value":5.5,
        "discount_scope":"services","discount_amount":0.67,
        "created_at":"2026-07-15T09:00:00Z","updated_at":"2026-07-15T10:00:00Z"}
        """;

        var quote = JsonHelper.Deserialize<Quote>(json)!;

        Assert.Equal(QuoteStatus.Sent, quote.Status);
        Assert.Equal(12.34m, quote.Subtotal);
        Assert.Equal(8.1m, quote.TaxRate);
        Assert.Null(quote.AcceptedAt);
        Assert.Equal("Ada", quote.ClientSnapshot!.Value.GetProperty("display_name").GetString());
        Assert.Equal(QuoteDiscountKind.Percent, quote.DiscountKind);
        Assert.Equal(QuoteDiscountScope.Services, quote.DiscountScope);
        var serialized = JsonHelper.Serialize(quote);
        Assert.Contains("\"company_id\":\"c1\"", serialized, StringComparison.Ordinal);
        Assert.Contains("\"status\":\"sent\"", serialized, StringComparison.Ordinal);
        Assert.Contains("\"discount_scope\":\"services\"", serialized, StringComparison.Ordinal);
    }

    [Fact]
    public void QuoteItem_DeserializesAllPricingSnapshots()
    {
        const string json = """
        {"id":"i1","quote_id":"q1","catalog_item_id":"cat1","description":"Line",
        "quantity":2.5,"unit":"h","unit_price":10.1,"line_total":25.25,"position":3,
        "line_kind":"labor","price_tax_mode_snapshot":"tax_included","vat_rate_snapshot":8.1,
        "discount_snapshot":2,"unit_price_input":10.1,"unit_price_net":9.34,
        "unit_price_gross":10.1,"net_amount":22.89,"vat_amount":1.86,"gross_amount":24.75,
        "created_at":"2026-07-15T09:00:00Z"}
        """;

        var item = JsonHelper.Deserialize<QuoteItem>(json)!;

        Assert.Equal(QuoteLineKind.Labor, item.LineKind);
        Assert.Equal(2.5m, item.Quantity);
        Assert.Equal(10.1m, item.UnitPriceInput);
        Assert.Equal(9.34m, item.UnitPriceNet);
        Assert.Equal(10.1m, item.UnitPriceGross);
        Assert.Equal(22.89m, item.NetAmount);
        Assert.Equal(1.86m, item.VatAmount);
        Assert.Equal(24.75m, item.GrossAmount);
    }

    [Fact]
    public void Enums_SerializeAsVerifiedLowercaseStrings()
    {
        Assert.Equal("\"draft\"", JsonHelper.Serialize(QuoteStatus.Draft));
        Assert.Equal("\"fixed\"", JsonHelper.Serialize(QuoteDiscountKind.Fixed));
        Assert.Equal("\"products\"", JsonHelper.Serialize(QuoteDiscountScope.Products));
        Assert.Equal("\"other\"", JsonHelper.Serialize(QuoteLineKind.Other));
    }

    [Fact]
    public void Enums_RejectUnknownStringsAndAllowNullableNulls()
    {
        Assert.Throws<JsonException>(() => JsonHelper.Deserialize<QuoteStatus>("\"future\""));
        Assert.Null(JsonHelper.Deserialize<QuoteStatus?>("null"));
        Assert.Null(JsonHelper.Deserialize<QuoteDiscountKind?>("null"));
    }

    [Fact]
    public void QuoteWithItems_TakesReadOnlySnapshot()
    {
        var source = new List<QuoteItem> { new() { Id = "first" } };
        var result = new QuoteWithItems(new Quote { Id = "q1" }, source);
        source.Add(new QuoteItem { Id = "second" });

        Assert.Single(result.Items);
        Assert.Equal("first", result.Items[0].Id);
    }

    [Fact]
    public async Task GetAsync_UsesExactEscapedIdAndReturnsVisibleQuote()
    {
        using var fixture = CreateFixture(JsonResponse("[{\"id\":\"a/b c\",\"status\":\"draft\"}]"));

        var quote = await fixture.Service.GetAsync("a/b c");

        Assert.Equal("a/b c", quote?.Id);
        var request = Assert.Single(fixture.Handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Contains("/rest/v1/quotes?select=id,company_id", request.Uri.OriginalString, StringComparison.Ordinal);
        Assert.Contains("id=eq.a%2Fb%20c", request.Uri.OriginalString, StringComparison.Ordinal);
        Assert.Contains("limit=1", request.Uri.Query, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetAsync_EmptyResultReturnsNull()
    {
        using var fixture = CreateFixture(JsonResponse("[]"));
        Assert.Null(await fixture.Service.GetAsync("missing"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task IdOperations_RejectMissingId(string? id)
    {
        using var fixture = CreateFixture(JsonResponse("[]"));
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.GetAsync(id!));
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.GetWithItemsAsync(id!));
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.ExistsAsync(id!));
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.DeleteAsync(id!));
        Assert.Empty(fixture.Handler.Requests);
    }

    [Fact]
    public async Task GetWithItemsAsync_UsesTwoRequestsAndOrdersItems()
    {
        using var fixture = CreateFixture(attempt => attempt == 1
            ? JsonResponse("[{\"id\":\"q/1\",\"company_id\":\"c1\",\"status\":\"draft\"}]")
            : JsonResponse("[{\"id\":\"i1\",\"quote_id\":\"q/1\",\"description\":\"A\"}]"));

        var result = await fixture.Service.GetWithItemsAsync("q/1");

        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(2, fixture.Handler.Requests.Count);
        var itemRequest = fixture.Handler.Requests[1];
        Assert.Contains("/rest/v1/quote_items", itemRequest.Uri.AbsolutePath, StringComparison.Ordinal);
        Assert.Contains("quote_id=eq.q%2F1", itemRequest.Uri.OriginalString, StringComparison.Ordinal);
        Assert.Contains("order=position.asc", itemRequest.Uri.Query, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetWithItemsAsync_EmptyItemsReturnsEmptyCollection()
    {
        using var fixture = CreateFixture(attempt => attempt == 1
            ? JsonResponse("[{\"id\":\"q1\",\"status\":\"draft\"}]")
            : JsonResponse("null"));
        Assert.Empty((await fixture.Service.GetWithItemsAsync("q1"))!.Items);
    }

    [Fact]
    public async Task GetWithItemsAsync_MissingQuoteSkipsItemRequest()
    {
        using var fixture = CreateFixture(JsonResponse("[]"));
        Assert.Null(await fixture.Service.GetWithItemsAsync("missing"));
        Assert.Single(fixture.Handler.Requests);
    }

    [Fact]
    public async Task GetWithItemsAsync_PropagatesCancellationFromItemRequest()
    {
        using var fixture = CreateFixture((attempt, _, token) => attempt == 1
            ? JsonResponse("[{\"id\":\"q1\",\"status\":\"draft\"}]")
            : throw new OperationCanceledException(token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => fixture.Service.GetWithItemsAsync("q1", new CancellationToken(true)));
    }

    [Fact]
    public async Task ListAsync_UsesVerifiedFieldsAndCreationOrderingOnly()
    {
        using var fixture = CreateFixture(JsonResponse("[{\"id\":\"q1\",\"status\":\"draft\"}]"));
        Assert.Single(await fixture.Service.ListAsync());
        var query = Assert.Single(fixture.Handler.Requests).Uri.Query;
        Assert.Contains("order=created_at.desc", query, StringComparison.Ordinal);
        Assert.DoesNotContain("limit=", query, StringComparison.Ordinal);
        Assert.DoesNotContain("offset=", query, StringComparison.Ordinal);
        Assert.DoesNotContain("company_id=", query, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ListAsync_NullReturnsEmptyAndRetriesTransientGet()
    {
        using var fixture = CreateFixture(attempt => attempt == 1
            ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            : JsonResponse("null"), 3);
        Assert.Empty(await fixture.Service.ListAsync());
        Assert.Equal(2, fixture.Handler.Requests.Count);
    }

    [Theory]
    [InlineData("[{\"id\":\"q1\"}]", true)]
    [InlineData("[]", false)]
    [InlineData("null", false)]
    public async Task ExistsAsync_UsesLightweightExactQuery(string json, bool expected)
    {
        using var fixture = CreateFixture(JsonResponse(json));
        Assert.Equal(expected, await fixture.Service.ExistsAsync("a/b"));
        var request = Assert.Single(fixture.Handler.Requests);
        Assert.Equal("?select=id&id=eq.a%2Fb&limit=1", request.Uri.Query);
    }

    [Fact]
    public async Task ExistsAsync_RetriesTransientGet()
    {
        using var fixture = CreateFixture(attempt => attempt == 1
            ? new HttpResponseMessage(HttpStatusCode.BadGateway)
            : JsonResponse("[]"), 3);
        Assert.False(await fixture.Service.ExistsAsync("q1"));
        Assert.Equal(2, fixture.Handler.Requests.Count);
    }

    [Theory]
    [InlineData(42)]
    [InlineData(0)]
    public async Task CountAsync_UsesHeadExactCountAndRange(long total)
    {
        var response = new HttpResponseMessage(HttpStatusCode.PartialContent);
        response.Content.Headers.ContentRange = new ContentRangeHeaderValue(0, 0, total);
        using var fixture = CreateFixture(response);
        Assert.Equal(total, await fixture.Service.CountAsync());
        var request = Assert.Single(fixture.Handler.Requests);
        Assert.Equal(HttpMethod.Head, request.Method);
        Assert.Equal("count=exact", Assert.Single(request.Headers["Prefer"]));
        Assert.Equal("bytes=0-0", Assert.Single(request.Headers["Range"]));
        Assert.Equal("?select=id", request.Uri.Query);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CountAsync_MissingOrMalformedContentRangeThrows(bool malformed)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        if (malformed) response.Content.Headers.TryAddWithoutValidation("Content-Range", "broken");
        using var fixture = CreateFixture(response);
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Service.CountAsync());
    }

    [Fact]
    public async Task CountAsync_RetriesTransientHead()
    {
        using var fixture = CreateFixture(attempt =>
        {
            var response = new HttpResponseMessage(attempt == 1 ? HttpStatusCode.GatewayTimeout : HttpStatusCode.PartialContent);
            if (attempt > 1) response.Content.Headers.ContentRange = new ContentRangeHeaderValue(0, 0, 3);
            return response;
        }, 3);
        Assert.Equal(3, await fixture.Service.CountAsync());
        Assert.Equal(2, fixture.Handler.Requests.Count);
    }

    [Fact]
    public async Task CountAsync_PropagatesCancellation()
    {
        using var fixture = CreateFixture((_, _, token) => throw new OperationCanceledException(token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => fixture.Service.CountAsync(new CancellationToken(true)));
    }

    public static TheoryData<QuoteStatus, QuoteStatus> AllowedTransitions => new()
    {
        { QuoteStatus.Draft, QuoteStatus.Sent }, { QuoteStatus.Draft, QuoteStatus.Accepted },
        { QuoteStatus.Draft, QuoteStatus.Rejected }, { QuoteStatus.Draft, QuoteStatus.Expired },
        { QuoteStatus.Sent, QuoteStatus.Accepted }, { QuoteStatus.Sent, QuoteStatus.Rejected },
        { QuoteStatus.Sent, QuoteStatus.Expired }, { QuoteStatus.Sent, QuoteStatus.Draft },
        { QuoteStatus.Expired, QuoteStatus.Sent }
    };

    [Theory]
    [MemberData(nameof(AllowedTransitions))]
    public async Task UpdateStatusAsync_AllowsExactlyVerifiedTransitions(QuoteStatus current, QuoteStatus target)
    {
        using var fixture = StatusFixture(current, target);
        var updated = await fixture.Service.UpdateStatusAsync("q/1", target);
        Assert.Equal(target, updated.Status);
        Assert.Equal(2, fixture.Handler.Requests.Count);
        var patch = fixture.Handler.Requests[1];
        Assert.Equal(HttpMethod.Patch, patch.Method);
        Assert.Contains("id=eq.q%2F1", patch.Uri.OriginalString, StringComparison.Ordinal);
        Assert.Contains("company_id=eq.c%2F1", patch.Uri.OriginalString, StringComparison.Ordinal);
        Assert.Equal("return=representation", Assert.Single(patch.Headers["Prefer"]));
    }

    public static TheoryData<QuoteStatus, QuoteStatus> ForbiddenTransitions => new()
    {
        { QuoteStatus.Accepted, QuoteStatus.Draft }, { QuoteStatus.Accepted, QuoteStatus.Sent },
        { QuoteStatus.Accepted, QuoteStatus.Rejected }, { QuoteStatus.Accepted, QuoteStatus.Expired },
        { QuoteStatus.Rejected, QuoteStatus.Draft }, { QuoteStatus.Rejected, QuoteStatus.Sent },
        { QuoteStatus.Rejected, QuoteStatus.Accepted }, { QuoteStatus.Rejected, QuoteStatus.Expired },
        { QuoteStatus.Expired, QuoteStatus.Draft }, { QuoteStatus.Expired, QuoteStatus.Accepted },
        { QuoteStatus.Expired, QuoteStatus.Rejected }
    };

    [Theory]
    [MemberData(nameof(ForbiddenTransitions))]
    public async Task UpdateStatusAsync_RejectsForbiddenTransitionsWithoutPatch(QuoteStatus current, QuoteStatus target)
    {
        using var fixture = CreateFixture(JsonResponse(CurrentQuoteJson(current)));
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Service.UpdateStatusAsync("q1", target));
        Assert.Single(fixture.Handler.Requests);
    }

    [Theory]
    [InlineData(QuoteStatus.Draft)]
    [InlineData(QuoteStatus.Sent)]
    [InlineData(QuoteStatus.Accepted)]
    [InlineData(QuoteStatus.Rejected)]
    [InlineData(QuoteStatus.Expired)]
    public async Task UpdateStatusAsync_SameStateReturnsCurrentWithoutPatch(QuoteStatus status)
    {
        using var fixture = CreateFixture(JsonResponse(CurrentQuoteJson(status)));
        var result = await fixture.Service.UpdateStatusAsync("q1", status);
        Assert.Equal(status, result.Status);
        Assert.Single(fixture.Handler.Requests);
    }

    [Theory]
    [InlineData(QuoteStatus.Sent, "sent_at")]
    [InlineData(QuoteStatus.Accepted, "accepted_at")]
    [InlineData(QuoteStatus.Rejected, "rejected_at")]
    public async Task UpdateStatusAsync_WritesRequiredTimestampOnly(QuoteStatus target, string timestampName)
    {
        using var fixture = StatusFixture(QuoteStatus.Draft, target);
        await fixture.Service.UpdateStatusAsync("q1", target);
        using var payload = JsonDocument.Parse(fixture.Handler.Requests[1].Body!);
        Assert.Equal(target.ToString().ToLowerInvariant(), payload.RootElement.GetProperty("status").GetString());
        Assert.True(payload.RootElement.TryGetProperty(timestampName, out _));
        foreach (var name in new[] { "sent_at", "accepted_at", "rejected_at" }.Where(x => x != timestampName))
            Assert.False(payload.RootElement.TryGetProperty(name, out _));
    }

    [Fact]
    public async Task UpdateStatusAsync_ResendPreservesSentAtAndDoesNotMutateCurrentModel()
    {
        var original = DateTimeOffset.Parse("2026-07-01T10:00:00Z");
        using var fixture = CreateFixture(attempt => attempt == 1
            ? JsonResponse(CurrentQuoteJson(QuoteStatus.Expired, sentAt: original))
            : JsonResponse(CurrentQuoteJson(QuoteStatus.Sent, sentAt: original)));
        await fixture.Service.UpdateStatusAsync("q1", QuoteStatus.Sent);
        using var payload = JsonDocument.Parse(fixture.Handler.Requests[1].Body!);
        Assert.False(payload.RootElement.TryGetProperty("sent_at", out _));
    }

    [Theory]
    [InlineData(QuoteStatus.Draft)]
    [InlineData(QuoteStatus.Expired)]
    public async Task UpdateStatusAsync_DraftAndExpiredInventNoTimestamp(QuoteStatus target)
    {
        var current = target == QuoteStatus.Draft ? QuoteStatus.Sent : QuoteStatus.Draft;
        using var fixture = StatusFixture(current, target);
        await fixture.Service.UpdateStatusAsync("q1", target);
        using var payload = JsonDocument.Parse(fixture.Handler.Requests[1].Body!);
        Assert.Single(payload.RootElement.EnumerateObject());
        Assert.False(payload.RootElement.TryGetProperty("expired_at", out _));
    }

    [Fact]
    public async Task UpdateStatusAsync_DoesNotRetryPatch()
    {
        using var fixture = CreateFixture(attempt => attempt == 1
            ? JsonResponse(CurrentQuoteJson(QuoteStatus.Draft))
            : new HttpResponseMessage(HttpStatusCode.ServiceUnavailable), 3);
        await Assert.ThrowsAsync<ApiException>(() => fixture.Service.UpdateStatusAsync("q1", QuoteStatus.Sent));
        Assert.Equal(2, fixture.Handler.Requests.Count);
    }

    [Fact]
    public async Task UpdateStatusAsync_MissingVisibleQuoteThrows()
    {
        using var fixture = CreateFixture(JsonResponse("[]"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Service.UpdateStatusAsync("q1", QuoteStatus.Sent));
    }

    [Fact]
    public async Task UpdateStatusAsync_PropagatesCancellationFromPatch()
    {
        using var fixture = CreateFixture((attempt, _, token) => attempt == 1
            ? JsonResponse(CurrentQuoteJson(QuoteStatus.Draft))
            : throw new OperationCanceledException(token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => fixture.Service.UpdateStatusAsync("q1", QuoteStatus.Sent, new CancellationToken(true)));
    }

    [Fact]
    public async Task DeleteAsync_UsesExactEscapedIdAndRetriesUntilSuccess()
    {
        using var fixture = CreateFixture(attempt => new HttpResponseMessage(
            attempt == 1 ? HttpStatusCode.TooManyRequests : HttpStatusCode.NoContent), 3);
        await fixture.Service.DeleteAsync("q/1");
        Assert.Equal(2, fixture.Handler.Requests.Count);
        Assert.All(fixture.Handler.Requests, request =>
        {
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal("?id=eq.q%2F1", request.Uri.Query);
        });
    }

    [Fact]
    public async Task DeleteAsync_StopsAfterSuccess()
    {
        using var fixture = CreateFixture(new HttpResponseMessage(HttpStatusCode.NoContent), 3);
        await fixture.Service.DeleteAsync("q1");
        Assert.Single(fixture.Handler.Requests);
    }

    [Fact]
    public async Task DeleteAsync_PropagatesCancellation()
    {
        using var fixture = CreateFixture((_, _, token) => throw new OperationCanceledException(token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => fixture.Service.DeleteAsync("q1", new CancellationToken(true)));
    }

    [Fact]
    public async Task Operations_PropagateApiErrorsCancellationAndDisposal()
    {
        using (var errorFixture = CreateFixture(new HttpResponseMessage(HttpStatusCode.Forbidden)
               { Content = new StringContent("denied") }))
        {
            var error = await Assert.ThrowsAsync<ApiException>(() => errorFixture.Service.GetAsync("q1"));
            Assert.Equal(HttpStatusCode.Forbidden, error.StatusCode);
        }

        using (var cancellationFixture = CreateFixture((_, _, token) => throw new OperationCanceledException(token)))
        using (var source = new CancellationTokenSource())
        {
            source.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => cancellationFixture.Service.ListAsync(source.Token));
        }

        var disposedFixture = CreateFixture(JsonResponse("[]"));
        disposedFixture.Client.Dispose();
        await Assert.ThrowsAsync<ObjectDisposedException>(() => disposedFixture.Service.GetAsync("q1"));
        disposedFixture.Dispose();
    }

    [Fact]
    public void ConstructorAndClientusClient_RespectLifetime()
    {
        Assert.Throws<ArgumentNullException>(() => new QuotesService(null!));
        using var client = new ClientusClient(Configuration());
        Assert.Same(client.Quotes, client.Quotes);
        client.Dispose();
        Assert.Throws<ObjectDisposedException>(() => client.Quotes);
    }

    private static Fixture StatusFixture(QuoteStatus current, QuoteStatus target) =>
        CreateFixture(attempt => attempt == 1
            ? JsonResponse(CurrentQuoteJson(current))
            : JsonResponse(CurrentQuoteJson(target)));

    private static string CurrentQuoteJson(QuoteStatus status, DateTimeOffset? sentAt = null) =>
        $"[{{\"id\":\"q1\",\"company_id\":\"c/1\",\"status\":\"{status.ToString().ToLowerInvariant()}\",\"sent_at\":{(sentAt is null ? "null" : $"\"{sentAt:O}\"")}}}]";

    private static Fixture CreateFixture(HttpResponseMessage response, int maxRetryAttempts = 1) =>
        CreateFixture(_ => response, maxRetryAttempts);

    private static Fixture CreateFixture(Func<int, HttpResponseMessage> factory, int maxRetryAttempts = 1) =>
        CreateFixture((attempt, _, _) => factory(attempt), maxRetryAttempts);

    private static Fixture CreateFixture(
        Func<int, HttpRequestMessage, CancellationToken, HttpResponseMessage> factory,
        int maxRetryAttempts = 1)
    {
        var handler = new RecordingHandler(factory);
        var client = new ClientusHttpClient(Configuration(maxRetryAttempts), handler);
        return new Fixture(client, new QuotesService(client), handler);
    }

    private static ClientusConfiguration Configuration(int maxRetryAttempts = 1) => new()
    {
        BaseUrl = "https://api.example.test",
        ApiKey = "test-key",
        MaxRetryAttempts = maxRetryAttempts,
        InitialRetryDelay = TimeSpan.Zero
    };

    private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    private sealed record RequestSnapshot(HttpMethod Method, Uri Uri, IReadOnlyDictionary<string, string[]> Headers, string? Body);

    private sealed class RecordingHandler(
        Func<int, HttpRequestMessage, CancellationToken, HttpResponseMessage> factory) : HttpMessageHandler
    {
        public List<RequestSnapshot> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var headers = request.Headers
                .Concat(request.Content?.Headers ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>() )
                .ToDictionary(x => x.Key, x => x.Value.ToArray(), StringComparer.OrdinalIgnoreCase);
            var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add(new RequestSnapshot(request.Method, request.RequestUri!, headers, body));
            return factory(Requests.Count, request, cancellationToken);
        }
    }

    private sealed class Fixture(ClientusHttpClient client, QuotesService service, RecordingHandler handler) : IDisposable
    {
        public ClientusHttpClient Client { get; } = client;
        public QuotesService Service { get; } = service;
        public RecordingHandler Handler { get; } = handler;
        public void Dispose() => Client.Dispose();
    }
}
