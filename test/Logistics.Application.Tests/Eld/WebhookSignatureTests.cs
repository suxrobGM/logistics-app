using System.Security.Cryptography;
using System.Text;
using Logistics.Infrastructure.Integrations.Common;
using Logistics.Application.Tests.TestKit;
using Xunit;

namespace Logistics.Application.Tests.Eld;

public class WebhookSignatureTests
{
    private const string HmacSecret = "shh-very-secret";
    private const string HmacPayload = """{"eventType":"violationCreated","driverId":"abc"}""";

    private const string SvixSecret = "whsec_MfKQ9r8GKYqrTwjUPD8ILPZIo2LaLaSw";
    private const string SvixPayload = """{"type":"email.received","data":{"email_id":"abc"}}""";
    private const string SvixMessageId = "msg_2sT4y";

    #region VerifyHmacSha256

    [Fact]
    public void VerifyHmacSha256_ValidSignature_ReturnsTrue()
    {
        var signature = WebhookTestKit.ComputeHmacHex(HmacPayload, HmacSecret);
        Assert.True(WebhookSignature.VerifyHmacSha256(HmacPayload, signature, HmacSecret));
    }

    [Fact]
    public void VerifyHmacSha256_UppercaseSignature_StillMatches()
    {
        var signature = WebhookTestKit.ComputeHmacHex(HmacPayload, HmacSecret).ToUpperInvariant();
        Assert.True(WebhookSignature.VerifyHmacSha256(HmacPayload, signature, HmacSecret));
    }

    [Fact]
    public void VerifyHmacSha256_TamperedPayload_ReturnsFalse()
    {
        var signature = WebhookTestKit.ComputeHmacHex(HmacPayload, HmacSecret);
        Assert.False(WebhookSignature.VerifyHmacSha256(HmacPayload + "extra", signature, HmacSecret));
    }

    [Fact]
    public void VerifyHmacSha256_WrongSecret_ReturnsFalse()
    {
        var signature = WebhookTestKit.ComputeHmacHex(HmacPayload, HmacSecret);
        Assert.False(WebhookSignature.VerifyHmacSha256(HmacPayload, signature, "different-secret"));
    }

    [Theory]
    [InlineData(null, "secret")]
    [InlineData("", "secret")]
    [InlineData("sig", null)]
    [InlineData("sig", "")]
    public void VerifyHmacSha256_MissingInputs_ReturnsFalse(string? signature, string? secret)
    {
        Assert.False(WebhookSignature.VerifyHmacSha256(HmacPayload, signature, secret));
    }

    #endregion

    #region VerifySvix

    [Fact]
    public void VerifySvix_ValidSignature_ReturnsTrue()
    {
        var timestamp = SvixNow();
        var header = SignSvix(SvixMessageId, timestamp, SvixPayload, SvixSecret);

        Assert.True(WebhookSignature.VerifySvix(SvixPayload, SvixMessageId, timestamp, header, SvixSecret));
    }

    [Fact]
    public void VerifySvix_SecretWithoutPrefix_ReturnsTrue()
    {
        var timestamp = SvixNow();
        var header = SignSvix(SvixMessageId, timestamp, SvixPayload, SvixSecret);

        Assert.True(WebhookSignature.VerifySvix(
            SvixPayload, SvixMessageId, timestamp, header, SvixSecret["whsec_".Length..]));
    }

    [Fact]
    public void VerifySvix_MultipleVersionsInHeader_MatchesAny()
    {
        var timestamp = SvixNow();
        var header = "v1,bm90LXRoZS1yaWdodC1vbmU= " + SignSvix(SvixMessageId, timestamp, SvixPayload, SvixSecret);

        Assert.True(WebhookSignature.VerifySvix(SvixPayload, SvixMessageId, timestamp, header, SvixSecret));
    }

    [Fact]
    public void VerifySvix_TamperedPayload_ReturnsFalse()
    {
        var timestamp = SvixNow();
        var header = SignSvix(SvixMessageId, timestamp, SvixPayload, SvixSecret);

        Assert.False(WebhookSignature.VerifySvix(SvixPayload + " ", SvixMessageId, timestamp, header, SvixSecret));
    }

    [Fact]
    public void VerifySvix_DifferentMessageId_ReturnsFalse()
    {
        var timestamp = SvixNow();
        var header = SignSvix(SvixMessageId, timestamp, SvixPayload, SvixSecret);

        Assert.False(WebhookSignature.VerifySvix(SvixPayload, "msg_other", timestamp, header, SvixSecret));
    }

    [Fact]
    public void VerifySvix_StaleTimestamp_ReturnsFalse()
    {
        var timestamp = DateTimeOffset.UtcNow.AddMinutes(-10).ToUnixTimeSeconds().ToString();
        var header = SignSvix(SvixMessageId, timestamp, SvixPayload, SvixSecret);

        Assert.False(WebhookSignature.VerifySvix(SvixPayload, SvixMessageId, timestamp, header, SvixSecret));
    }

    [Fact]
    public void VerifySvix_FutureTimestampBeyondTolerance_ReturnsFalse()
    {
        var timestamp = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds().ToString();
        var header = SignSvix(SvixMessageId, timestamp, SvixPayload, SvixSecret);

        Assert.False(WebhookSignature.VerifySvix(SvixPayload, SvixMessageId, timestamp, header, SvixSecret));
    }

    [Fact]
    public void VerifySvix_UnknownVersionOnly_ReturnsFalse()
    {
        var timestamp = SvixNow();
        var header = SignSvix(SvixMessageId, timestamp, SvixPayload, SvixSecret).Replace("v1,", "v2,");

        Assert.False(WebhookSignature.VerifySvix(SvixPayload, SvixMessageId, timestamp, header, SvixSecret));
    }

    [Theory]
    [InlineData(null, "1", "v1,sig", SvixSecret)]
    [InlineData(SvixMessageId, null, "v1,sig", SvixSecret)]
    [InlineData(SvixMessageId, "1", null, SvixSecret)]
    [InlineData(SvixMessageId, "1", "v1,sig", null)]
    [InlineData(SvixMessageId, "not-a-number", "v1,sig", SvixSecret)]
    public void VerifySvix_MissingOrMalformedInputs_ReturnsFalse(
        string? id, string? timestamp, string? signature, string? secret)
    {
        Assert.False(WebhookSignature.VerifySvix(SvixPayload, id, timestamp, signature, secret));
    }

    #endregion

    #region VerifySamsara

    /// <summary>The example secret from Samsara's webhook signature guide.</summary>
    private const string SamsaraSecret = "rGoy+beNph0qGBLj6Aqoydj6SGA=";
    private const string SamsaraPayload = """{"eventId":"bf72239c","eventType":"Ping","event":{"text":"Ping"}}""";

    [Fact]
    public void VerifySamsara_ValidSignature_ReturnsTrue()
    {
        var timestamp = SvixNow();
        var header = SignSamsara(timestamp, SamsaraPayload, SamsaraSecret);

        Assert.True(WebhookSignature.VerifySamsara(SamsaraPayload, timestamp, header, SamsaraSecret));
    }

    [Fact]
    public void VerifySamsara_SignatureOverBodyOnly_ReturnsFalse()
    {
        var timestamp = SvixNow();
        var bodyOnly = "v1=" + Convert.ToHexStringLower(HMACSHA256.HashData(
            Convert.FromBase64String(SamsaraSecret), Encoding.UTF8.GetBytes(SamsaraPayload)));

        Assert.False(WebhookSignature.VerifySamsara(SamsaraPayload, timestamp, bodyOnly, SamsaraSecret));
    }

    [Fact]
    public void VerifySamsara_SecretUsedUndecoded_ReturnsFalse()
    {
        var timestamp = SvixNow();
        var header = "v1=" + Convert.ToHexStringLower(HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(SamsaraSecret), Encoding.UTF8.GetBytes($"v1:{timestamp}:{SamsaraPayload}")));

        Assert.False(WebhookSignature.VerifySamsara(SamsaraPayload, timestamp, header, SamsaraSecret));
    }

    [Fact]
    public void VerifySamsara_TamperedPayload_ReturnsFalse()
    {
        var timestamp = SvixNow();
        var header = SignSamsara(timestamp, SamsaraPayload, SamsaraSecret);

        Assert.False(WebhookSignature.VerifySamsara(SamsaraPayload + " ", timestamp, header, SamsaraSecret));
    }

    [Fact]
    public void VerifySamsara_ReplayedOutsideWindow_ReturnsFalse()
    {
        var timestamp = DateTimeOffset.UtcNow.AddMinutes(-10).ToUnixTimeSeconds().ToString();
        var header = SignSamsara(timestamp, SamsaraPayload, SamsaraSecret);

        Assert.False(WebhookSignature.VerifySamsara(SamsaraPayload, timestamp, header, SamsaraSecret));
    }

    [Fact]
    public void VerifySamsara_TimestampSwappedForFreshOne_ReturnsFalse()
    {
        var header = SignSamsara("1587597109", SamsaraPayload, SamsaraSecret);

        Assert.False(WebhookSignature.VerifySamsara(SamsaraPayload, SvixNow(), header, SamsaraSecret));
    }

    [Fact]
    public void VerifySamsara_WithinWindowOfInjectedClock_ReturnsTrue()
    {
        const string timestamp = "1587597109";
        var header = SignSamsara(timestamp, SamsaraPayload, SamsaraSecret);
        var now = DateTimeOffset.FromUnixTimeSeconds(1587597109).AddMinutes(2);

        Assert.True(WebhookSignature.VerifySamsara(SamsaraPayload, timestamp, header, SamsaraSecret, now));
    }

    [Theory]
    [InlineData(null, "v1=abc", SamsaraSecret)]
    [InlineData("1587597109", null, SamsaraSecret)]
    [InlineData("1587597109", "v1=abc", null)]
    [InlineData("not-a-number", "v1=abc", SamsaraSecret)]
    [InlineData("1587597109", "v2=abc", SamsaraSecret)]
    [InlineData("1587597109", "v1=abc", "not base64!")]
    public void VerifySamsara_MissingOrMalformedInputs_ReturnsFalse(
        string? timestamp, string? signature, string? secret)
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(1587597109);
        Assert.False(WebhookSignature.VerifySamsara(SamsaraPayload, timestamp, signature, secret, now));
    }

    #endregion

    private static string SvixNow() => DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

    private static string SignSvix(string id, string timestamp, string payload, string secret)
    {
        var key = Convert.FromBase64String(secret["whsec_".Length..]);
        var content = Encoding.UTF8.GetBytes($"{id}.{timestamp}.{payload}");
        return "v1," + Convert.ToBase64String(HMACSHA256.HashData(key, content));
    }

    private static string SignSamsara(string timestamp, string payload, string base64Secret)
    {
        var content = Encoding.UTF8.GetBytes($"v1:{timestamp}:{payload}");
        return "v1=" + Convert.ToHexStringLower(HMACSHA256.HashData(Convert.FromBase64String(base64Secret), content));
    }
}
