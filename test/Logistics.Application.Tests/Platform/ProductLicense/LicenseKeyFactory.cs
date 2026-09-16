using System.Security.Cryptography;
using Logistics.Application.Abstractions.ProductLicense;
using Microsoft.IdentityModel.Tokens;

namespace Logistics.Application.Tests.Platform.ProductLicense;

/// <summary>
/// Signs test license keys with an ephemeral P-256 key pair, the same way the issuer tool does.
/// </summary>
internal sealed class LicenseKeyFactory : IDisposable
{
    private readonly ECDsa _signer = ECDsa.Create(ECCurve.NamedCurves.nistP256);

    public string PublicKey => Convert.ToBase64String(_signer.ExportSubjectPublicKeyInfo());

    public string Sign(
        DateTime? expires = null,
        string issuer = ProductLicenseClaims.Issuer,
        string audience = ProductLicenseClaims.Audience,
        string? tier = "Hosted",
        string licensee = "Acme Freight",
        int? maxTenants = null,
        string? keyId = "2026-09",
        SigningCredentials? credentials = null)
    {
        return ProductLicenseToken.Sign(
            credentials ?? ProductLicenseToken.CreateSigningCredentials(_signer, keyId),
            licensee,
            tier,
            expires ?? DateTime.UtcNow.AddYears(1),
            maxTenants,
            issuer,
            audience);
    }

    public void Dispose() => _signer.Dispose();
}
