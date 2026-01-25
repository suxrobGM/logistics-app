using System.Security.Cryptography;

namespace Logistics.Application.Utilities;

/// <summary>
/// Utility for generating secure random tokens.
/// </summary>
public static class TokenGenerator
{
    /// <summary>
    /// Generates a URL-safe secure random token.
    /// </summary>
    /// <param name="byteLength">Number of random bytes to generate. Default is 32.</param>
    /// <returns>A URL-safe base64-encoded token.</returns>
    public static string GenerateSecureToken(int byteLength = 32)
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(byteLength))
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
