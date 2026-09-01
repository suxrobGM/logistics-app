namespace Logistics.Application.Modules.Platform.ProductLicense.Services;

/// <summary>
/// The author's license signing public key (P-256, SubjectPublicKeyInfo, base64). Keys are
/// signed by tools/Logistics.LicenseIssuer with the matching private key, which never enters
/// the repo. To rotate: run the issuer's keygen, replace this constant, and reissue keys.
/// </summary>
internal static class ProductLicensePublicKey
{
    public const string SpkiBase64 = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEomP4+dnZcBRXzzbJ7ov8lZBlwV3yJDil5EC4x0zP8kl9lbElMAQGgy37buo54nlDumSZwMAvZWFtb1Gh1vu/Tw==";
}
