using System.Security.Cryptography;
using System.Text;
using Logistics.Application.Abstractions.Email;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Communications.Email;

/// <summary>Resend signs webhooks with Svix.</summary>
internal sealed class ResendWebhookVerifier(IOptions<ResendOptions> options) : IInboundEmailWebhookVerifier
{
    /// <summary>How far the signed timestamp may be from now, in either direction.</summary>
    private static readonly TimeSpan TimestampTolerance = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Checks the base64 HMAC-SHA256 of <c>{id}.{timestamp}.{payload}</c>, keyed by the decoded
    /// <c>whsec_</c> secret. Fails closed on anything missing, malformed or stale.
    /// </summary>
    public bool Verify(string rawBody, string? svixId, string? svixTimestamp, string? svixSignatureHeader)
    {
        var secret = options.Value.WebhookSecret;

        if (string.IsNullOrEmpty(svixId) ||
            string.IsNullOrEmpty(svixTimestamp) ||
            string.IsNullOrEmpty(svixSignatureHeader) ||
            string.IsNullOrEmpty(secret))
        {
            return false;
        }

        if (!long.TryParse(svixTimestamp, out var unixSeconds))
        {
            return false;
        }

        var age = DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        if (age.Duration() > TimestampTolerance)
        {
            return false;
        }

        if (!TryDecodeSecret(secret, out var key))
        {
            return false;
        }

        var signedContent = Encoding.UTF8.GetBytes($"{svixId}.{svixTimestamp}.{rawBody}");
        var expected = Encoding.UTF8.GetBytes(Convert.ToBase64String(HMACSHA256.HashData(key, signedContent)));

        // The header carries every signature currently valid for the endpoint, so a secret being
        // rotated has two. Any match is a pass.
        foreach (var entry in svixSignatureHeader.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!entry.StartsWith("v1,", StringComparison.Ordinal))
            {
                continue;
            }

            var provided = Encoding.UTF8.GetBytes(entry[3..]);
            if (provided.Length == expected.Length &&
                CryptographicOperations.FixedTimeEquals(provided, expected))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryDecodeSecret(string secret, out byte[] key)
    {
        const string prefix = "whsec_";
        var raw = secret.StartsWith(prefix, StringComparison.Ordinal) ? secret[prefix.Length..] : secret;

        var buffer = new byte[raw.Length];
        if (Convert.TryFromBase64String(raw, buffer, out var written) && written > 0)
        {
            key = buffer[..written];
            return true;
        }

        key = [];
        return false;
    }
}
