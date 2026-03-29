using System.Security.Cryptography;
using System.Text;

namespace Logistics.Application.Utilities;

public static class ApiKeyHasher
{
    /// <summary>
    /// Hashes the raw API key using SHA256 and returns a lowercase hex string.
    /// </summary>
    public static string Hash(string rawKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
        return Convert.ToHexStringLower(bytes);
    }
}
