using System.Security.Cryptography;
using System.Text;

namespace Logistics.Infrastructure.Integrations.Common;

/// <summary>
/// Helpers for verifying inbound webhook HMAC signatures with constant-time
/// comparison (per the project security rules).
/// </summary>
public static class WebhookSignature
{
    /// <summary>How far a signed timestamp (Svix, Samsara) may be from now, in either direction.</summary>
    private static readonly TimeSpan SignedTimestampTolerance = TimeSpan.FromMinutes(5);

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

        return HexHmacMatches(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(payload), signatureHex);
    }

    /// <summary>
    /// Verifies a Svix-signed webhook (Resend and others): base64 HMAC-SHA256 of
    /// <c>{id}.{timestamp}.{payload}</c>, keyed by the base64-decoded secret with its
    /// <c>whsec_</c> prefix removed. Fails closed on anything missing, malformed, or stale.
    /// </summary>
    public static bool VerifySvix(
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

        if (!IsFresh(unixSeconds, DateTimeOffset.UtcNow))
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

    /// <summary>
    /// Verifies a Samsara-signed webhook: <c>X-Samsara-Signature: v1=&lt;hex&gt;</c> where the hex is the
    /// HMAC-SHA256 of <c>v1:{X-Samsara-Timestamp}:{body}</c>, keyed by the base64-decoded webhook secret.
    /// Fails closed on anything missing, malformed, or older/newer than the tolerance window.
    /// </summary>
    public static bool VerifySamsara(
        string payload,
        string? timestamp,
        string? signatureHeader,
        string? base64Secret,
        DateTimeOffset? now = null)
    {
        const string prefix = "v1=";
        if (string.IsNullOrEmpty(timestamp) ||
            string.IsNullOrEmpty(signatureHeader) ||
            string.IsNullOrEmpty(base64Secret) ||
            !signatureHeader.StartsWith(prefix, StringComparison.Ordinal) ||
            !long.TryParse(timestamp, out var unixSeconds))
        {
            return false;
        }

        if (!IsFresh(unixSeconds, now ?? DateTimeOffset.UtcNow))
        {
            return false;
        }

        var buffer = new byte[base64Secret.Length];
        if (!Convert.TryFromBase64String(base64Secret.Trim(), buffer, out var written) || written == 0)
        {
            return false;
        }

        var message = Encoding.UTF8.GetBytes($"v1:{timestamp}:{payload}");
        return HexHmacMatches(buffer[..written], message, signatureHeader[prefix.Length..]);
    }

    private static bool IsFresh(long unixSeconds, DateTimeOffset now) =>
        (now - DateTimeOffset.FromUnixTimeSeconds(unixSeconds)).Duration() <= SignedTimestampTolerance;

    /// <summary>Constant-time check that <paramref name="providedHex"/> is the lowercase hex HMAC-SHA256 of the message.</summary>
    private static bool HexHmacMatches(byte[] key, byte[] message, string providedHex)
    {
        var expected = Encoding.UTF8.GetBytes(Convert.ToHexStringLower(HMACSHA256.HashData(key, message)));
        var provided = Encoding.UTF8.GetBytes(providedHex.Trim().ToLowerInvariant());
        return provided.Length == expected.Length && CryptographicOperations.FixedTimeEquals(provided, expected);
    }

    /// <summary>
    /// Constant-time equality for a shared-secret header. Fails closed when either side is
    /// missing, so an unconfigured secret rejects every caller instead of accepting every caller.
    /// </summary>
    public static bool ConstantTimeEquals(string? provided, string? expected)
    {
        if (string.IsNullOrEmpty(provided) || string.IsNullOrEmpty(expected))
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(provided), Encoding.UTF8.GetBytes(expected));
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
