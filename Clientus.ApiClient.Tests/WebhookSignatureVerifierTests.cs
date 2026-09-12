using Clientus.ApiClient.Webhooks;
using Xunit;

namespace Clientus.ApiClient.Tests;

public sealed class WebhookSignatureVerifierTests
{
    [Fact]
    public void Valid_signature_is_accepted()
    {
        const string secret = "test-secret";
        const string body = "{\"ok\":true}";
        const long unix = 1_700_000_000;

        using var hmac = new System.Security.Cryptography.HMACSHA256(
            System.Text.Encoding.UTF8.GetBytes(secret));

        var expected = Convert.ToHexString(
            hmac.ComputeHash(
                System.Text.Encoding.UTF8.GetBytes($"{unix}.{body}")))
            .ToLowerInvariant();

        var header = $"t={unix},v=v1,sig={expected}";

        var result = WebhookSignatureVerifier.Verify(
            body,
            header,
            secret,
            TimeSpan.FromMinutes(5),
            DateTimeOffset.FromUnixTimeSeconds(unix));

        Assert.True(result);
    }

    [Fact]
    public void Old_timestamp_is_rejected()
    {
        const string body = "{}";
        const string secret = "test-secret";
        const string header =
            "t=1000,v=v1,sig=0000000000000000000000000000000000000000000000000000000000000000";

        Assert.False(
            WebhookSignatureVerifier.Verify(
                body,
                header,
                secret,
                TimeSpan.FromMinutes(5),
                DateTimeOffset.FromUnixTimeSeconds(10_000)));
    }
}