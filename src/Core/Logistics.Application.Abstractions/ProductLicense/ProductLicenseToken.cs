using System.Security.Cryptography;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Logistics.Application.Abstractions.ProductLicense;

/// <summary>
/// Signs and validates license keys. One home for the token shape, so the product, the issuer
/// tool, and the tests cannot disagree about what a valid key looks like.
/// </summary>
public static class ProductLicenseToken
{
    /// <summary>Tolerance applied to the expiry check, since issuer and instance clocks differ.</summary>
    public static readonly TimeSpan ClockSkew = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Lifetime validation is off on purpose: the caller checks expiry itself so an expired key
    /// still reports who it belonged to.
    /// </summary>
    public static TokenValidationParameters CreateValidationParameters(ECDsa publicKey) => new()
    {
        ValidIssuer = ProductLicenseClaims.Issuer,
        ValidAudience = ProductLicenseClaims.Audience,
        IssuerSigningKey = new ECDsaSecurityKey(publicKey),
        ValidAlgorithms = [SecurityAlgorithms.EcdsaSha256],
        RequireSignedTokens = true,
        RequireExpirationTime = true,
        ValidateLifetime = false,
        ClockSkew = ClockSkew
    };

    public static SigningCredentials CreateSigningCredentials(ECDsa privateKey, string? keyId) =>
        new(new ECDsaSecurityKey(privateKey) { KeyId = keyId }, SecurityAlgorithms.EcdsaSha256);

    /// <summary>
    /// Issuer and audience are overridable so the tests can produce keys the validator must reject.
    /// </summary>
    public static string Sign(
        SigningCredentials credentials,
        string licensee,
        string? tier,
        DateTime expiresUtc,
        int? maxTenants = null,
        string issuer = ProductLicenseClaims.Issuer,
        string audience = ProductLicenseClaims.Audience)
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

        var now = DateTime.UtcNow;
        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = expiresUtc,
            Claims = claims,
            SigningCredentials = credentials
        });
    }
}
