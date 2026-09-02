using System.Security.Cryptography;
using Logistics.Application.Abstractions.ProductLicense;
using Logistics.Domain.Primitives.Enums;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Logistics.Application.Modules.Platform.ProductLicense.Services;

/// <summary>
/// Checks a license key's signature, issuer, audience, and claims against the embedded public
/// key. Expiry is checked separately so an expired key still reports who it belonged to.
/// </summary>
internal sealed class ProductLicenseKeyValidator
{
    private readonly JsonWebTokenHandler handler = new();
    private readonly TokenValidationParameters parameters;

    public ProductLicenseKeyValidator(string spkiBase64)
    {
        var ecdsa = ECDsa.Create();
        ecdsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(spkiBase64), out _);

        parameters = ProductLicenseToken.CreateValidationParameters(ecdsa);
    }

    public async Task<ProductLicenseValidationResult> ValidateAsync(string key, DateTime? nowUtc = null)
    {
        var result = await handler.ValidateTokenAsync(key.Trim(), parameters);
        if (!result.IsValid)
        {
            return ProductLicenseValidationResult.Invalid(DescribeFailure(result.Exception));
        }

        var jwt = (JsonWebToken)result.SecurityToken;
        jwt.TryGetPayloadValue<string>(ProductLicenseClaims.Licensee, out var licensee);
        jwt.TryGetPayloadValue<string>(ProductLicenseClaims.Tier, out var tierName);
        var maxTenants = jwt.TryGetPayloadValue<int>(ProductLicenseClaims.MaxTenants, out var cap) ? cap : (int?)null;
        var keyId = string.IsNullOrEmpty(jwt.Kid) ? null : jwt.Kid;

        if (!Enum.TryParse<ProductLicenseTier>(tierName, ignoreCase: true, out var tier))
        {
            return ProductLicenseValidationResult.Invalid($"unknown tier '{tierName}'");
        }

        var expired = jwt.ValidTo.Add(ProductLicenseToken.ClockSkew) < (nowUtc ?? DateTime.UtcNow);

        return new ProductLicenseValidationResult(
            IsValid: !expired,
            Error: expired ? "expired" : null,
            Licensee: licensee,
            Tier: tier,
            ExpiresAt: jwt.ValidTo,
            MaxTenants: maxTenants,
            KeyId: keyId);
    }

    private static string DescribeFailure(Exception? exception) => exception switch
    {
        SecurityTokenInvalidSignatureException or SecurityTokenSignatureKeyNotFoundException => "invalid signature",
        SecurityTokenInvalidAlgorithmException => "invalid algorithm",
        SecurityTokenInvalidIssuerException => "invalid issuer",
        SecurityTokenInvalidAudienceException => "invalid audience",
        SecurityTokenNoExpirationException => "missing expiry",
        _ => "malformed key"
    };
}

internal sealed record ProductLicenseValidationResult(
    bool IsValid,
    string? Error,
    string? Licensee,
    ProductLicenseTier? Tier,
    DateTime? ExpiresAt,
    int? MaxTenants,
    string? KeyId)
{
    public static ProductLicenseValidationResult Invalid(string error) =>
        new(false, error, null, null, null, null, null);
}
