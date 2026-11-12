using Logistics.Infrastructure.Persistence.Converters;
using Microsoft.AspNetCore.DataProtection;
using Xunit;

namespace Logistics.Infrastructure.Persistence.Tests.Converters;

public class EncryptedStringConverterTests
{
    private static EncryptedStringConverter CreateConverter() =>
        new(new EphemeralDataProtectionProvider().CreateProtector("test"));

    private static string Encrypt(EncryptedStringConverter c, string value) =>
        (string)c.ConvertToProvider(value)!;

    private static string Decrypt(EncryptedStringConverter c, string stored) =>
        (string)c.ConvertFromProvider(stored)!;

    [Fact]
    public void RoundTrips_PlaintextThroughEncryptAndDecrypt()
    {
        var converter = CreateConverter();
        const string secret = "logsx_super_secret_token";

        var stored = Encrypt(converter, secret);
        var recovered = Decrypt(converter, stored);

        Assert.Equal(secret, recovered);
    }

    [Fact]
    public void Encrypt_ProducesPrefixedCiphertext_NotPlaintext()
    {
        var converter = CreateConverter();

        var stored = Encrypt(converter, "my-secret");

        Assert.StartsWith(EncryptedStringConverter.Prefix, stored);
        Assert.DoesNotContain("my-secret", stored);
    }

    [Fact]
    public void Decrypt_LegacyPlaintextWithoutPrefix_ReturnsUnchanged()
    {
        var converter = CreateConverter();

        // A value written before encryption existed has no prefix and must pass through.
        var result = Decrypt(converter, "legacy-plaintext-apikey");

        Assert.Equal("legacy-plaintext-apikey", result);
    }

    [Fact]
    public void Encrypt_AlreadyEncrypted_IsNotDoubleEncrypted()
    {
        var converter = CreateConverter();
        var once = Encrypt(converter, "secret");

        var twice = Encrypt(converter, once);

        Assert.Equal(once, twice);
        Assert.Equal("secret", Decrypt(converter, twice));
    }

    [Fact]
    public void Encrypt_SameValueTwice_ProducesDifferentCiphertext()
    {
        var converter = CreateConverter();

        var a = Encrypt(converter, "secret");
        var b = Encrypt(converter, "secret");

        // Non-deterministic by design (why lookup-by-value tokens are excluded from encryption).
        Assert.NotEqual(a, b);
        Assert.Equal("secret", Decrypt(converter, a));
        Assert.Equal("secret", Decrypt(converter, b));
    }
}
