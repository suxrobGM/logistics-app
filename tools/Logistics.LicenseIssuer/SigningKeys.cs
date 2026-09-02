using System.Security.Cryptography;

namespace Logistics.LicenseIssuer;

/// <summary>
/// Loads the ECDSA keys the tool signs and inspects with. The private key comes from the
/// environment and never touches the repo.
/// </summary>
internal static class SigningKeys
{
    public const string PrivateKeyVariable = "LOGISTICSX_LICENSE_PRIVATE_KEY";

    /// <summary>Returns null when the variable is unset, so the caller can print its own hint.</summary>
    public static ECDsa? LoadPrivateKey()
    {
        var pkcs8 = Environment.GetEnvironmentVariable(PrivateKeyVariable);
        if (string.IsNullOrWhiteSpace(pkcs8))
        {
            return null;
        }

        var key = ECDsa.Create();
        key.ImportPkcs8PrivateKey(Convert.FromBase64String(pkcs8), out _);
        return key;
    }

    public static ECDsa LoadPublicKey(string spkiBase64)
    {
        var key = ECDsa.Create();
        key.ImportSubjectPublicKeyInfo(Convert.FromBase64String(spkiBase64), out _);
        return key;
    }
}
