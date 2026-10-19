using System.Security.Cryptography;
using System.Text;

namespace Logistics.Application.Utilities;

public static class TokenHasher
{
    /// <summary>
    /// Hashes a raw secret (API key, invitation or impersonation token) with SHA256 and returns a lowercase hex string.
    /// Only the hash is stored; the raw value goes out once, in the link or response.
    /// </summary>
    public static string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexStringLower(bytes);
    }
}
