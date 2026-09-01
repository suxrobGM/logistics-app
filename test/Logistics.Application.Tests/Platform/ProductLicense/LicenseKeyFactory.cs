using System.Security.Cryptography;
using Logistics.Application.Abstractions.ProductLicense;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Logistics.Application.Tests.Platform.ProductLicense;

/// <summary>
/// Signs test license keys with an ephemeral P-256 key pair, the same way the issuer tool does.
/// </summary>
internal sealed class LicenseKeyFactory : IDisposable
{
    private readonly ECDsa signer = ECDsa.Create(ECCurve.NamedCurves.nistP256);

    public string PublicKey => Convert.ToBase64String(signer.ExportSubjectPublicKeyInfo());

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
        var claims = new Dictionary<string, object> { [ProductLicenseClaims.Licensee] = licensee };
        if (tier is not null)
        {
            claims[ProductLicenseClaims.Tier] = tier;
        }

        if (maxTenants is { } cap)
        {
            claims[ProductLicenseClaims.MaxTenants] = cap;
        }

        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            Expires = expires ?? DateTime.UtcNow.AddYears(1),
            Claims = claims,
            SigningCredentials = credentials ?? new SigningCredentials(
                new ECDsaSecurityKey(signer) { KeyId = keyId },
                SecurityAlgorithms.EcdsaSha256)
        });
    }

    public void Dispose() => signer.Dispose();
}
