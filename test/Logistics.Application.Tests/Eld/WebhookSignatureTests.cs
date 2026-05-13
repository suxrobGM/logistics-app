using System.Security.Cryptography;
using System.Text;
using Logistics.Infrastructure.Integrations.Eld.Common;
using Xunit;

namespace Logistics.Application.Tests.Eld;

public class WebhookSignatureTests
{
    private const string secret = "shh-very-secret";
    private const string payload = """{"eventType":"violationCreated","driverId":"abc"}""";

    [Fact]
    public void VerifyHmacSha256_ValidSignature_ReturnsTrue()
    {
        var signature = ComputeHex(payload, secret);
        Assert.True(WebhookSignature.VerifyHmacSha256(payload, signature, secret));
    }

    [Fact]
    public void VerifyHmacSha256_UppercaseSignature_StillMatches()
    {
        var signature = ComputeHex(payload, secret).ToUpperInvariant();
        Assert.True(WebhookSignature.VerifyHmacSha256(payload, signature, secret));
    }

    [Fact]
    public void VerifyHmacSha256_TamperedPayload_ReturnsFalse()
    {
        var signature = ComputeHex(payload, secret);
        Assert.False(WebhookSignature.VerifyHmacSha256(payload + "extra", signature, secret));
    }

    [Fact]
    public void VerifyHmacSha256_WrongSecret_ReturnsFalse()
    {
        var signature = ComputeHex(payload, secret);
        Assert.False(WebhookSignature.VerifyHmacSha256(payload, signature, "different-secret"));
    }

    [Theory]
    [InlineData(null, "secret")]
    [InlineData("", "secret")]
    [InlineData("sig", null)]
    [InlineData("sig", "")]
    public void VerifyHmacSha256_MissingInputs_ReturnsFalse(string? signature, string? secret)
    {
        Assert.False(WebhookSignature.VerifyHmacSha256(payload, signature, secret));
    }

    private static string ComputeHex(string payload, string secret)
    {
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexStringLower(hash);
    }
}
