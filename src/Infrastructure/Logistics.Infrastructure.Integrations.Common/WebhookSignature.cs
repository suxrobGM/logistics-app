using System.Security.Cryptography;
using System.Text;

namespace Logistics.Infrastructure.Integrations.Common;

/// <summary>
/// Helpers for verifying inbound webhook HMAC signatures with constant-time
/// comparison (per the project security rules).
/// </summary>
public static class WebhookSignature
{
    /// <summary>
    /// Verifies that <paramref name="signatureHex"/> equals the lowercase hex
    /// HMAC-SHA256 of <paramref name="payload"/> using <paramref name="secret"/>.
    /// </summary>
    public static bool VerifyHmacSha256(string payload, string? signatureHex, string? secret)
    {
        if (string.IsNullOrEmpty(signatureHex) || string.IsNullOrEmpty(secret))
        {
            return false;
        }

        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var computed = HMACSHA256.HashData(keyBytes, payloadBytes);
        var expected = Convert.ToHexStringLower(computed);
        var providedBytes = Encoding.UTF8.GetBytes(signatureHex.Trim().ToLowerInvariant());
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        return providedBytes.Length == expectedBytes.Length
               && CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
    }

    /// <summary>
    /// Verifies a Svix-signed webhook (Resend and others): base64 HMAC-SHA256 of
    /// <c>{id}.{timestamp}.{payload}</c>, keyed by the base64-decoded secret with its
    /// <c>whsec_</c> prefix removed. Fails closed on anything missing, malformed, or stale.
    /// </summary>
    /// <param name="tolerance">
    /// How far the signed timestamp may be from now, in either direction. Svix recommends 5 minutes.
    /// </param>
    public static bool VerifySvix(
        string payload,
        string? svixId,
        string? svixTimestamp,
        string? svixSignatureHeader,
        string? secret,
        TimeSpan? tolerance = null)
    {
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

        var window = tolerance ?? TimeSpan.FromMinutes(5);
        var age = DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        if (age.Duration() > window)
        {
            return false;
        }

        if (!TryDecodeSecret(secret, out var key))
        {
            return false;
        }

        var signedContent = Encoding.UTF8.GetBytes($"{svixId}.{svixTimestamp}.{payload}");
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
