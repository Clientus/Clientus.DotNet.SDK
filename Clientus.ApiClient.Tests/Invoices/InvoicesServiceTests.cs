using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Clientus.ApiClient.Common;
using Clientus.ApiClient.Configuration;
using Clientus.ApiClient.Http;
using Clientus.ApiClient.Invoices;
using Clientus.ApiClient.Serialization;

namespace Clientus.ApiClient.Tests.Invoices;

public sealed class InvoicesServiceTests
{
    [Fact]
    public void Models_MapVerifiedJsonDecimalsSnapshotsAndEnums()
    {
        const string json = """
        {"id":"i1","company_id":"c1","status":"partial","kind":"deposit","currency":"CHF",
        "subtotal":100.25,"tax_rate":8.1,"tax_amount":8.12,"total":108.37,"issued_at":"2026-07-15",
        "sent_at":null,"paid_at":null,"client_snapshot":{"name":"Ada"},"public_token":"p",
        "paid_amount":20.5,"remaining_amount":87.87,"discount_kind":"percent","discount_scope":"services",
        "installment_cadence":"monthly","is_installment_source":false,"payment_account_snapshot":{"iban":"CH1"},
        "reminders_sent":{},"created_at":"2026-07-15T09:00:00Z","updated_at":"2026-07-15T10:00:00Z"}
        """;
        var value = JsonHelper.Deserialize<Invoice>(json)!;
        Assert.Equal(InvoiceStatus.Partial, value.Status);
        Assert.Equal(InvoiceKind.Deposit, value.Kind);
        Assert.Equal(108.37m, value.Total);
        Assert.Null(value.SentAt);
        Assert.Equal("Ada", value.ClientSnapshot!.Value.GetProperty("name").GetString());
        Assert.Contains("\"company_id\":\"c1\"", JsonHelper.Serialize(value), StringComparison.Ordinal);
    }

    [Fact]
    public void ItemAndEnums_UseVerifiedSerializationPolicy()
    {
        const string json = """
        {"id":"x","invoice_id":"i","description":"Work","quantity":2.5,"unit_price":10.1,
        "line_total":25.25,"position":2,"line_kind":"labor","vat_rate_snapshot":8.1,
        "unit_price_net":9.34,"vat_amount":1.86,"created_at":"2026-07-15T09:00:00Z"}
        """;
        var item = JsonHelper.Deserialize<InvoiceItem>(json)!;
        Assert.Equal(InvoiceLineKind.Labor, item.LineKind);
        Assert.Equal(1.86m, item.VatAmount);
        Assert.Equal("\"overdue\"", JsonHelper.Serialize(InvoiceStatus.Overdue));
        Assert.Null(JsonHelper.Deserialize<InvoiceKind?>("null"));
        Assert.Throws<JsonException>(() => JsonHelper.Deserialize<InvoiceStatus>("\"future\""));
        Assert.Equal("\"999\"", JsonHelper.Serialize((InvoiceStatus)999));
    }

    [Fact]
    public void WithItems_TakesReadOnlySnapshot()
    {
        var source = new List<InvoiceItem> { new() { Id = "one" } };
        var result = new InvoiceWithItems(new Invoice(), source);
        source.Add(new InvoiceItem());
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task Get_UsesExactEscapedFilterAndHandlesEmptyAndErrors()
    {
        using (var found = Fixture.Create(Json("[{\"id\":\"a/b c\",\"status\":\"draft\",\"kind\":\"standard\"}]")))
        {
            Assert.Equal("a/b c", (await found.Service.GetAsync("a/b c"))?.Id);
            Assert.Contains("id=eq.a%2Fb%20c", found.Handler.Requests.Single().Uri.OriginalString, StringComparison.Ordinal);
        }
        using (var empty = Fixture.Create(Json("[]"))) Assert.Null(await empty.Service.GetAsync("missing"));
        using var denied = Fixture.Create(new HttpResponseMessage(HttpStatusCode.Forbidden) { Content = new StringContent("denied") });
        var error = await Assert.ThrowsAsync<ApiException>(() => denied.Service.GetAsync("i"));
        Assert.Equal(HttpStatusCode.Forbidden, error.StatusCode);
        Assert.Equal("denied", error.ResponseBody);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task IdentifierOperations_ValidateConsistently(string? id)
    {
        using var f = Fixture.Create(Json("[]"));
        await Assert.ThrowsAsync<ArgumentException>(() => f.Service.GetAsync(id!));
        await Assert.ThrowsAsync<ArgumentException>(() => f.Service.GetWithItemsAsync(id!));
        await Assert.ThrowsAsync<ArgumentException>(() => f.Service.ExistsAsync(id!));
        await Assert.ThrowsAsync<ArgumentException>(() => f.Service.DeleteAsync(id!));
        Assert.Empty(f.Handler.Requests);
    }

    [Fact]
    public async Task GetWithItems_UsesTwoRequestsAndPositionOrdering()
    {
        using var f = Fixture.Create(n => n == 1
            ? Json("[{\"id\":\"i/1\",\"status\":\"draft\",\"kind\":\"standard\"}]")
            : Json("[{\"id\":\"x\",\"invoice_id\":\"i/1\",\"description\":\"A\"}]"));
        Assert.Single((await f.Service.GetWithItemsAsync("i/1"))!.Items);
        Assert.Contains("invoice_id=eq.i%2F1", f.Handler.Requests[1].Uri.OriginalString, StringComparison.Ordinal);
        Assert.Contains("order=position.asc", f.Handler.Requests[1].Uri.Query, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetWithItems_MissingSkipsSecondRequestAndNullItemsAreEmpty()
    {
        using (var missing = Fixture.Create(Json("[]")))
        {
            Assert.Null(await missing.Service.GetWithItemsAsync("i"));
            Assert.Single(missing.Handler.Requests);
        }
        using var empty = Fixture.Create(n => n == 1
            ? Json("[{\"id\":\"i\",\"status\":\"draft\",\"kind\":\"standard\"}]") : Json("null"));
        Assert.Empty((await empty.Service.GetWithItemsAsync("i"))!.Items);
    }

    [Fact]
    public async Task List_OrdersByCreationAndRetriesTransientGetWithFreshRequests()
    {
        using var f = Fixture.Create(n => n == 1 ? new(HttpStatusCode.ServiceUnavailable) : Json("null"), 3);
        Assert.Empty(await f.Service.ListAsync());
        Assert.Equal(2, f.Handler.Requests.Count);
        Assert.NotSame(f.Handler.Requests[0].Request, f.Handler.Requests[1].Request);
        var query = f.Handler.Requests[1].Uri.Query;
        Assert.Contains("order=created_at.desc", query, StringComparison.Ordinal);
        Assert.DoesNotContain("limit=", query, StringComparison.Ordinal);
        Assert.DoesNotContain("company_id=", query, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("[{\"id\":\"i\"}]", true)]
    [InlineData("[]", false)]
    [InlineData("null", false)]
    public async Task Exists_UsesLightweightExactQuery(string json, bool expected)
    {
        using var f = Fixture.Create(Json(json));
        Assert.Equal(expected, await f.Service.ExistsAsync("a/b"));
        Assert.Equal("?select=id&id=eq.a%2Fb&limit=1", f.Handler.Requests.Single().Uri.Query);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    public async Task Count_UsesRetryableHeadExactCountAndRange(long total)
    {
        var response = new HttpResponseMessage(HttpStatusCode.PartialContent);
        response.Content.Headers.ContentRange = new ContentRangeHeaderValue(0, 0, total);
        using var f = Fixture.Create(response);
        Assert.Equal(total, await f.Service.CountAsync());
        var request = f.Handler.Requests.Single();
        Assert.Equal(HttpMethod.Head, request.Method);
        Assert.Equal("count=exact", Assert.Single(request.Headers["Prefer"]));
        Assert.Equal("bytes=0-0", Assert.Single(request.Headers["Range"]));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Count_RejectsMissingOrMalformedContentRange(bool malformed)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        if (malformed) response.Content.Headers.TryAddWithoutValidation("Content-Range", "broken");
        using var f = Fixture.Create(response);
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Service.CountAsync());
    }

    [Fact]
    public async Task Delete_UsesExactFilterAndRetriesTransientFailureOnlyUntilSuccess()
    {
        using var f = Fixture.Create(n => new(n == 1 ? HttpStatusCode.TooManyRequests : HttpStatusCode.NoContent), 3);
        await f.Service.DeleteAsync("i/1");
        Assert.Equal(2, f.Handler.Requests.Count);
        Assert.All(f.Handler.Requests, r => Assert.Equal("?id=eq.i%2F1", r.Uri.Query));
    }

    [Fact]
    public async Task CancellationIsNotRetriedAndDisposedOperationsFail()
    {
        using var cancelled = Fixture.Create((_, _, token) => throw new OperationCanceledException(token), 3);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => cancelled.Service.ListAsync(new CancellationToken(true)));
        Assert.Single(cancelled.Handler.Requests);
        var disposed = Fixture.Create(Json("[]"));
        disposed.Client.Dispose();
        await Assert.ThrowsAsync<ObjectDisposedException>(() => disposed.Service.GetAsync("i"));
        disposed.Dispose();
    }

    [Fact]
    public void ConstructorAndParentClientExposeStableLifecycle()
    {
        Assert.Throws<ArgumentNullException>(() => new InvoicesService(null!));
        using var client = new ClientusClient(Config());
        Assert.Same(client.Invoices, client.Invoices);
        client.Dispose();
        Assert.Throws<ObjectDisposedException>(() => client.Invoices);
    }

    private static HttpResponseMessage Json(string value) => new(HttpStatusCode.OK)
        { Content = new StringContent(value, Encoding.UTF8, "application/json") };
    private static ClientusConfiguration Config(int retries = 1) => new()
        { BaseUrl = "https://api.example.test", ApiKey = "key", MaxRetryAttempts = retries, InitialRetryDelay = TimeSpan.Zero };
    private sealed record Snapshot(HttpMethod Method, Uri Uri, IReadOnlyDictionary<string, string[]> Headers, HttpRequestMessage Request);
    private sealed class Handler(Func<int, HttpRequestMessage, CancellationToken, HttpResponseMessage> factory) : HttpMessageHandler
    {
        public List<Snapshot> Requests { get; } = [];
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            var headers = request.Headers.Concat(request.Content?.Headers ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>())
                .ToDictionary(x => x.Key, x => x.Value.ToArray(), StringComparer.OrdinalIgnoreCase);
            Requests.Add(new(request.Method, request.RequestUri!, headers, request));
            return Task.FromResult(factory(Requests.Count, request, token));
        }
    }
    private sealed class Fixture(ClientusHttpClient client, InvoicesService service, Handler handler) : IDisposable
    {
        public ClientusHttpClient Client { get; } = client;
        public InvoicesService Service { get; } = service;
        public Handler Handler { get; } = handler;
        public static Fixture Create(HttpResponseMessage response, int retries = 1) => Create(_ => response, retries);
        public static Fixture Create(Func<int, HttpResponseMessage> factory, int retries = 1) => Create((n, _, _) => factory(n), retries);
        public static Fixture Create(Func<int, HttpRequestMessage, CancellationToken, HttpResponseMessage> factory, int retries = 1)
        {
            var handler = new Handler(factory);
            var client = new ClientusHttpClient(Config(retries), handler);
            return new(client, new InvoicesService(client), handler);
        }
        public void Dispose() => Client.Dispose();
    }
}
