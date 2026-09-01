using System.Security.Cryptography;
using System.Text;
using Logistics.Application.Modules.Platform.ProductLicense.Services;
using Logistics.Domain.Primitives.Enums;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Logistics.Application.Tests.Platform.ProductLicense;

public class ProductLicenseKeyValidatorTests : IDisposable
{
    private readonly LicenseKeyFactory keys = new();
    private readonly ProductLicenseKeyValidator sut;

    public ProductLicenseKeyValidatorTests()
    {
        sut = new ProductLicenseKeyValidator(keys.PublicKey);
    }

    public void Dispose() => keys.Dispose();

    [Fact]
    public async Task Validate_ValidKey_ReturnsLicensedWithClaims()
    {
        var expires = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var key = keys.Sign(expires, tier: "PerpetualSource", licensee: "Acme", maxTenants: 5, keyId: "k1");

        var result = await sut.ValidateAsync(key);

        Assert.True(result.IsValid);
        Assert.Null(result.Error);
        Assert.Equal("Acme", result.Licensee);
        Assert.Equal(ProductLicenseTier.PerpetualSource, result.Tier);
        Assert.Equal(expires, result.ExpiresAt);
        Assert.Equal(5, result.MaxTenants);
        Assert.Equal("k1", result.KeyId);
    }

    [Fact]
    public async Task Validate_ExpiredKey_ReturnsExpiredButKeepsLicensee()
    {
        var key = keys.Sign(DateTime.UtcNow.AddDays(-1), licensee: "Old Co");

        var result = await sut.ValidateAsync(key);

        Assert.False(result.IsValid);
        Assert.Equal("expired", result.Error);
        Assert.Equal("Old Co", result.Licensee);
        Assert.Equal(ProductLicenseTier.Hosted, result.Tier);
    }

    [Fact]
    public async Task Validate_SignedByOtherKey_ReturnsInvalidSignature()
    {
        using var other = new LicenseKeyFactory();

        var result = await sut.ValidateAsync(other.Sign());

        Assert.False(result.IsValid);
        Assert.Equal("invalid signature", result.Error);
        Assert.Null(result.Licensee);
    }

    [Fact]
    public async Task Validate_WrongIssuer_ReturnsInvalid()
    {
        var result = await sut.ValidateAsync(keys.Sign(issuer: "SomeoneElse"));

        Assert.False(result.IsValid);
        Assert.Equal("invalid issuer", result.Error);
    }

    [Fact]
    public async Task Validate_WrongAudience_ReturnsInvalid()
    {
        var result = await sut.ValidateAsync(keys.Sign(audience: "other-product"));

        Assert.False(result.IsValid);
        Assert.Equal("invalid audience", result.Error);
    }

    [Fact]
    public async Task Validate_Hs256Token_ReturnsInvalid()
    {
        var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(new string('s', 64)));
        var key = keys.Sign(credentials: new SigningCredentials(secret, SecurityAlgorithms.HmacSha256));

        var result = await sut.ValidateAsync(key);

        Assert.False(result.IsValid);
        Assert.Null(result.Licensee);
    }

    [Fact]
    public async Task Validate_UnknownTier_ReturnsInvalid()
    {
        var result = await sut.ValidateAsync(keys.Sign(tier: "Platinum"));

        Assert.False(result.IsValid);
        Assert.Contains("unknown tier", result.Error);
    }

    [Fact]
    public async Task Validate_Garbage_ReturnsInvalid()
    {
        var result = await sut.ValidateAsync("not-a-license-key");

        Assert.False(result.IsValid);
        Assert.Equal("malformed key", result.Error);
    }

    [Fact]
    public async Task Validate_WithinClockSkew_StillValid()
    {
        var expires = DateTime.UtcNow.AddMinutes(-1);

        var result = await sut.ValidateAsync(keys.Sign(expires), nowUtc: DateTime.UtcNow);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Constructor_InvalidPublicKey_Throws()
    {
        Assert.ThrowsAny<CryptographicException>(() =>
            new ProductLicenseKeyValidator(Convert.ToBase64String(new byte[] { 1, 2, 3 })));
    }
}
