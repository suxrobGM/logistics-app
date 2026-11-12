using System.Security.Cryptography;
using System.Text;

namespace Logistics.Infrastructure.Communications.Email;

/// <summary>Verifies Svix-signed webhooks, which is how Resend signs its webhooks.</summary>
public static class SvixSignature
{
    /// <summary>How far the signed timestamp may be from now, in either direction.</summary>
    private static readonly TimeSpan TimestampTolerance = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Verifies a base64 HMAC-SHA256 of <c>{id}.{timestamp}.{payload}</c>, keyed by the
    /// base64-decoded secret with its <c>whsec_</c> prefix removed. Fails closed on anything
    /// missing, malformed, or stale. The comparison is constant-time.
    /// </summary>
    public static bool Verify(
        string payload,
        string? svixId,
        string? svixTimestamp,
        string? svixSignatureHeader,
        string? secret)
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

        var age = DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        if (age.Duration() > TimestampTolerance)
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
