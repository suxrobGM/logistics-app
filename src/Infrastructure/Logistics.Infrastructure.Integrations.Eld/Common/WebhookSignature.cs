using System.Security.Cryptography;
using System.Text;

namespace Logistics.Infrastructure.Integrations.Eld.Common;

/// <summary>
/// Helpers for verifying inbound webhook HMAC signatures with constant-time
/// comparison (per the project security rules).
/// </summary>
internal static class WebhookSignature
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
}
