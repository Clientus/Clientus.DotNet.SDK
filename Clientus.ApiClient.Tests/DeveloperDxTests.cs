using Clientus.ApiClient;
using Clientus.ApiClient.Ai;
using Clientus.ApiClient.Webhooks;
using Xunit;

namespace Clientus.ApiClient.Tests;

public sealed class DeveloperDxTests
{
    [Fact]
    public void Client_exposes_expected_developer_services()
    {
        Assert.NotNull(typeof(ClientusClient).GetProperty("Customers"));
        Assert.NotNull(typeof(ClientusClient).GetProperty("Catalog"));
        Assert.NotNull(typeof(ClientusClient).GetProperty("Quotes"));
        Assert.NotNull(typeof(ClientusClient).GetProperty("Invoices"));
        Assert.NotNull(typeof(ClientusClient).GetProperty("Ai"));
    }

    [Fact]
    public void Webhook_verifier_is_public()
    {
        Assert.True(typeof(WebhookSignatureVerifier).IsPublic);
    }

    [Fact]
    public void Ai_service_is_public()
    {
        Assert.True(typeof(AiService).IsPublic);
    }
}