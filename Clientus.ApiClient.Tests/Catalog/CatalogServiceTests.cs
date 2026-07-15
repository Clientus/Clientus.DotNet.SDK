using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Clientus.ApiClient.Catalog;
using Clientus.ApiClient.Common;
using Clientus.ApiClient.Configuration;
using Clientus.ApiClient.Http;
using Clientus.ApiClient.Serialization;

namespace Clientus.ApiClient.Tests.Catalog;

public sealed class CatalogServiceTests
{
    [Fact]
    public void Models_MapVerifiedFieldsDecimalsNullsAndEnums()
    {
        const string json = """
        {"id":"p1","company_id":"c1","created_by":"u1","kind":"product","name":"Valve",
        "description":null,"category":"Parts","category_id":"cat1","unit":"pcs","price":12.345,
        "photo_url":null,"price_tax_mode":"tax_included","vat_rate":8.1,
        "created_at":"2026-07-15T09:00:00Z","updated_at":"2026-07-15T10:00:00Z"}
        """;
        var item = JsonHelper.Deserialize<CatalogItem>(json)!;
        Assert.Equal(CatalogItemType.Product, item.Kind);
        Assert.Equal(CatalogPriceTaxMode.TaxIncluded, item.PriceTaxMode);
        Assert.Equal(12.345m, item.Price);
        Assert.Null(item.Description);
        var serialized = JsonHelper.Serialize(item);
        Assert.Contains("\"company_id\":\"c1\"", serialized, StringComparison.Ordinal);
        Assert.Contains("\"price_tax_mode\":\"tax_included\"", serialized, StringComparison.Ordinal);
    }

    [Fact]
    public void CategoryAndEnums_RoundTripAndRejectUnknownNames()
    {
        const string json = """
        {"id":"cat","company_id":"c","name":"All","type":"both","icon":null,"color":"#fff",
        "sort_order":2,"is_default":true,"created_at":"2026-07-15T09:00:00Z","updated_at":"2026-07-15T10:00:00Z"}
        """;
        Assert.Equal(CatalogCategoryType.Both, JsonHelper.Deserialize<CatalogCategory>(json)!.Type);
        Assert.Equal("\"service\"", JsonHelper.Serialize(CatalogItemType.Service));
        Assert.Equal("\"no_tax\"", JsonHelper.Serialize(CatalogPriceTaxMode.NoTax));
        Assert.Null(JsonHelper.Deserialize<CatalogPriceTaxMode?>("null"));
        Assert.Throws<JsonException>(() => JsonHelper.Deserialize<CatalogItemType>("\"labor\""));
        Assert.Throws<JsonException>(() => JsonHelper.Deserialize<CatalogPriceTaxMode>("\"future\""));
    }

    [Fact]
    public async Task Get_UsesExactEscapedIdAndHandlesVisibleEmptyAndErrors()
    {
        using (var found = Fixture.Create(Json(ItemJson("a/b c"))))
        {
            Assert.Equal("a/b c", (await found.Service.GetAsync("a/b c"))!.Id);
            var request = Assert.Single(found.Handler.Requests);
            Assert.Contains("/rest/v1/catalog_items?select=id,company_id", request.Uri.OriginalString, StringComparison.Ordinal);
            Assert.Contains("id=eq.a%2Fb%20c", request.Uri.OriginalString, StringComparison.Ordinal);
            Assert.Contains("limit=1", request.Uri.Query, StringComparison.Ordinal);
        }
        using (var empty = Fixture.Create(Json("[]"))) Assert.Null(await empty.Service.GetAsync("missing"));
        using var denied = Fixture.Create(new HttpResponseMessage(HttpStatusCode.Forbidden)
            { Content = new StringContent("denied") });
        var error = await Assert.ThrowsAsync<ApiException>(() => denied.Service.GetAsync("p"));
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
        await Assert.ThrowsAsync<ArgumentException>(() => f.Service.ExistsAsync(id!));
        await Assert.ThrowsAsync<ArgumentException>(() => f.Service.DeleteAsync(id!));
        Assert.Empty(f.Handler.Requests);
    }

    [Fact]
    public async Task List_UsesVerifiedOrderingWithoutUnsupportedFiltersAndRetriesFreshGet()
    {
        using var f = Fixture.Create(n => n == 1 ? new(HttpStatusCode.ServiceUnavailable) : Json("null"), 3);
        Assert.Empty(await f.Service.ListAsync());
        Assert.Equal(2, f.Handler.Requests.Count);
        Assert.NotSame(f.Handler.Requests[0].Request, f.Handler.Requests[1].Request);
        var query = f.Handler.Requests[1].Uri.Query;
        Assert.Contains("order=kind.asc,name.asc", query, StringComparison.Ordinal);
        Assert.DoesNotContain("company_id=", query, StringComparison.Ordinal);
        Assert.DoesNotContain("limit=", query, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(CatalogItemType.Product, "product")]
    [InlineData(CatalogItemType.Service, "service")]
    public async Task GetByType_UsesExactKindAndNameOrder(CatalogItemType type, string value)
    {
        using var f = Fixture.Create(Json("[]"));
        Assert.Empty(await f.Service.GetByTypeAsync(type));
        Assert.Contains($"kind=eq.{value}", f.Handler.Requests.Single().Uri.Query, StringComparison.Ordinal);
        Assert.Contains("order=name.asc", f.Handler.Requests.Single().Uri.Query, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetByType_RejectsUndefinedEnumWithoutRequest()
    {
        using var f = Fixture.Create(Json("[]"));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => f.Service.GetByTypeAsync((CatalogItemType)99));
        Assert.Empty(f.Handler.Requests);
    }

    [Fact]
    public async Task Search_UsesVerifiedFieldsWildcardEscapingTypeOrderAndLimit()
    {
        using var f = Fixture.Create(Json("[]"));
        Assert.Empty(await f.Service.SearchAsync("  10%_off  ", 25, CatalogItemType.Service));
        var uri = f.Handler.Requests.Single().Uri.OriginalString;
        Assert.Contains("kind=eq.service", uri, StringComparison.Ordinal);
        Assert.Contains("name.ilike.%2510%5C%25%5C_off%25", uri, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("description.ilike.", uri, StringComparison.Ordinal);
        Assert.Contains("category.ilike.", uri, StringComparison.Ordinal);
        Assert.Contains("order=name.asc", uri, StringComparison.Ordinal);
        Assert.Contains("limit=25", uri, StringComparison.Ordinal);
        Assert.DoesNotContain("company_id=", uri, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public async Task Search_ValidatesLimit(int limit)
    {
        using var f = Fixture.Create(Json("[]"));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => f.Service.SearchAsync("x", limit));
        Assert.Empty(f.Handler.Requests);
    }

    [Fact]
    public async Task Search_WhitespaceReturnsEmptyWithoutRequest()
    {
        using var f = Fixture.Create(Json("[]"));
        Assert.Empty(await f.Service.SearchAsync("  "));
        Assert.Empty(f.Handler.Requests);
    }

    [Fact]
    public async Task Categories_UseVerifiedTableFieldsAndOrdering()
    {
        using var f = Fixture.Create(Json("[]"));
        Assert.Empty(await f.Service.ListCategoriesAsync());
        var uri = f.Handler.Requests.Single().Uri.OriginalString;
        Assert.Contains("/rest/v1/catalog_categories?select=id,company_id,name,type", uri, StringComparison.Ordinal);
        Assert.Contains("order=sort_order.asc,name.asc", uri, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("[{\"id\":\"p\"}]", true)]
    [InlineData("[]", false)]
    [InlineData("null", false)]
    public async Task Exists_UsesLightweightExactQueryAndRetries(string json, bool expected)
    {
        using var f = Fixture.Create(n => n == 1 ? new(HttpStatusCode.BadGateway) : Json(json), 2);
        Assert.Equal(expected, await f.Service.ExistsAsync("a/b"));
        Assert.Equal(2, f.Handler.Requests.Count);
        Assert.All(f.Handler.Requests, r => Assert.Equal("?select=id&id=eq.a%2Fb&limit=1", r.Uri.Query));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    public async Task Count_UsesHeadExactCountRangeAndRetries(long total)
    {
        using var f = Fixture.Create(n => n == 1 ? new(HttpStatusCode.GatewayTimeout) : CountResponse(total), 2);
        Assert.Equal(total, await f.Service.CountAsync());
        Assert.Equal(2, f.Handler.Requests.Count);
        var request = f.Handler.Requests[1];
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
    public async Task Update_SendsOnlyWritableFieldsDoesNotMutateAndDoesNotRetryPatch()
    {
        var input = Item("a/b");
        using var f = Fixture.Create(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable), 3);
        await Assert.ThrowsAsync<ApiException>(() => f.Service.UpdateAsync(input));
        var request = Assert.Single(f.Handler.Requests);
        Assert.Equal(HttpMethod.Patch, request.Method);
        Assert.Contains("id=eq.a%2Fb", request.Uri.OriginalString, StringComparison.Ordinal);
        Assert.Equal("return=representation", Assert.Single(request.Headers["Prefer"]));
        using var body = JsonDocument.Parse(request.Body!);
        foreach (var protectedName in new[] { "id", "company_id", "created_by", "created_at", "updated_at" })
            Assert.False(body.RootElement.TryGetProperty(protectedName, out _));
        Assert.Equal("product", body.RootElement.GetProperty("kind").GetString());
        Assert.Equal("tax_excluded", body.RootElement.GetProperty("price_tax_mode").GetString());
        Assert.Equal("c", input.CompanyId);
        Assert.Equal(DateTimeOffset.Parse("2026-01-01T00:00:00Z"), input.UpdatedAt);
    }

    [Fact]
    public async Task Update_ReturnsRepresentationAndValidatesInput()
    {
        using (var f = Fixture.Create(Json(ItemJson("p"))))
            Assert.Equal("p", (await f.Service.UpdateAsync(Item("p"))).Id);
        using var invalid = Fixture.Create(Json("[]"));
        await Assert.ThrowsAsync<ArgumentNullException>(() => invalid.Service.UpdateAsync(null!));
        await Assert.ThrowsAsync<ArgumentException>(() => invalid.Service.UpdateAsync(new CatalogItem()));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => invalid.Service.UpdateAsync(Item("p", (CatalogItemType)99)));
        Assert.Empty(invalid.Handler.Requests);
    }

    [Fact]
    public async Task Delete_UsesExactFilterRetriesAndStopsAfterSuccess()
    {
        using var f = Fixture.Create(n => new(n == 1 ? HttpStatusCode.TooManyRequests : HttpStatusCode.NoContent), 3);
        await f.Service.DeleteAsync("p/1");
        Assert.Equal(2, f.Handler.Requests.Count);
        Assert.All(f.Handler.Requests, r => Assert.Equal("?id=eq.p%2F1", r.Uri.Query));
    }

    [Fact]
    public async Task CancellationIsNotRetriedAndDisposedOperationsFail()
    {
        using var cancelled = Fixture.Create((_, _, token) => throw new OperationCanceledException(token), 3);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => cancelled.Service.CountAsync(new CancellationToken(true)));
        Assert.Single(cancelled.Handler.Requests);
        var disposed = Fixture.Create(Json("[]"));
        disposed.Client.Dispose();
        await Assert.ThrowsAsync<ObjectDisposedException>(() => disposed.Service.ListAsync());
        disposed.Dispose();
    }

    [Fact]
    public void ConstructorAndParentClientExposeStableLifecycle()
    {
        Assert.Throws<ArgumentNullException>(() => new CatalogService(null!));
        using var client = new ClientusClient(Config());
        Assert.Same(client.Catalog, client.Catalog);
        client.Dispose();
        Assert.Throws<ObjectDisposedException>(() => client.Catalog);
    }

    private static CatalogItem Item(string id, CatalogItemType type = CatalogItemType.Product) => new()
    {
        Id = id, CompanyId = "c", CreatedBy = "u", Kind = type, Name = "Valve", Description = "D",
        Category = "Parts", CategoryId = "cat", Unit = "pcs", Price = 12.34m, PhotoUrl = "photo",
        PriceTaxMode = CatalogPriceTaxMode.TaxExcluded, VatRate = 8.1m,
        CreatedAt = DateTimeOffset.Parse("2025-01-01T00:00:00Z"),
        UpdatedAt = DateTimeOffset.Parse("2026-01-01T00:00:00Z")
    };
    private static string ItemJson(string id) =>
        $"[{{\"id\":\"{id}\",\"kind\":\"product\",\"name\":\"Valve\",\"price_tax_mode\":\"inherit\"}}]";
    private static HttpResponseMessage Json(string value) => new(HttpStatusCode.OK)
        { Content = new StringContent(value, Encoding.UTF8, "application/json") };
    private static HttpResponseMessage CountResponse(long total)
    {
        var response = new HttpResponseMessage(HttpStatusCode.PartialContent);
        response.Content.Headers.ContentRange = new ContentRangeHeaderValue(0, 0, total);
        return response;
    }
    private static ClientusConfiguration Config(int retries = 1) => new()
        { BaseUrl = "https://api.example.test", ApiKey = "key", MaxRetryAttempts = retries, InitialRetryDelay = TimeSpan.Zero };
    private sealed record Snapshot(HttpMethod Method, Uri Uri, IReadOnlyDictionary<string, string[]> Headers, string? Body, HttpRequestMessage Request);
    private sealed class Handler(Func<int, HttpRequestMessage, CancellationToken, HttpResponseMessage> factory) : HttpMessageHandler
    {
        public List<Snapshot> Requests { get; } = [];
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            var headers = request.Headers.Concat(request.Content?.Headers ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>())
                .ToDictionary(x => x.Key, x => x.Value.ToArray(), StringComparer.OrdinalIgnoreCase);
            var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(token);
            Requests.Add(new(request.Method, request.RequestUri!, headers, body, request));
            return factory(Requests.Count, request, token);
        }
    }
    private sealed class Fixture(ClientusHttpClient client, CatalogService service, Handler handler) : IDisposable
    {
        public ClientusHttpClient Client { get; } = client;
        public CatalogService Service { get; } = service;
        public Handler Handler { get; } = handler;
        public static Fixture Create(HttpResponseMessage response, int retries = 1) => Create(_ => response, retries);
        public static Fixture Create(Func<int, HttpResponseMessage> factory, int retries = 1) => Create((n, _, _) => factory(n), retries);
        public static Fixture Create(Func<int, HttpRequestMessage, CancellationToken, HttpResponseMessage> factory, int retries = 1)
        {
            var handler = new Handler(factory);
            var client = new ClientusHttpClient(Config(retries), handler);
            return new(client, new CatalogService(client), handler);
        }
        public void Dispose() => Client.Dispose();
    }
}
