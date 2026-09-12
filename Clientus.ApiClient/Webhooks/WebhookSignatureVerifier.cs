using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Clientus.ApiClient.Webhooks;

public static class WebhookSignatureVerifier
{
    public static bool Verify(
        string rawBody,
        string signatureHeader,
        string signingSecret,
        TimeSpan? tolerance = null,
        DateTimeOffset? now = null)
    {
        if (string.IsNullOrWhiteSpace(rawBody) ||
            string.IsNullOrWhiteSpace(signatureHeader) ||
            string.IsNullOrWhiteSpace(signingSecret))
        {
            return false;
        }

        var parts = signatureHeader
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => part.Split('=', 2))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0], parts => parts[1], StringComparer.Ordinal);

        if (!parts.TryGetValue("t", out var timestampText) ||
            !parts.TryGetValue("sig", out var signature))
        {
            return false;
        }

        if (!long.TryParse(timestampText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var unix))
        {
            return false;
        }

        var timestamp = DateTimeOffset.FromUnixTimeSeconds(unix);
        var clock = now ?? DateTimeOffset.UtcNow;
        var allowed = tolerance ?? TimeSpan.FromMinutes(5);

        if (Math.Abs((clock - timestamp).TotalSeconds) > allowed.TotalSeconds)
        {
            return false;
        }

        var signedPayload = $"{unix}.{rawBody}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(signingSecret));
        var expected = Convert.ToHexString(
            hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload)))
            .ToLowerInvariant();

        try
        {
            return CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(expected),
                Encoding.ASCII.GetBytes(signature.ToLowerInvariant()));
        }
        catch
        {
            return false;
        }
    }
}