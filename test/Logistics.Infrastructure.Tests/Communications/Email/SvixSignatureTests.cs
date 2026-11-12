using System.Security.Cryptography;
using System.Text;
using Logistics.Infrastructure.Communications.Email;
using Xunit;

namespace Logistics.Infrastructure.Communications.Tests.Email;

public class SvixSignatureTests
{
    private const string Secret = "whsec_MfKQ9r8GKYqrTwjUPD8ILPZIo2LaLaSw";
    private const string Payload = """{"type":"email.received","data":{"email_id":"abc"}}""";
    private const string MessageId = "msg_2sT4y";

    [Fact]
    public void Verify_ValidSignature_ReturnsTrue()
    {
        var timestamp = Now();
        var header = Sign(MessageId, timestamp, Payload, Secret);

        Assert.True(SvixSignature.Verify(Payload, MessageId, timestamp, header, Secret));
    }

    [Fact]
    public void Verify_SecretWithoutPrefix_ReturnsTrue()
    {
        var timestamp = Now();
        var header = Sign(MessageId, timestamp, Payload, Secret);

        Assert.True(SvixSignature.Verify(
            Payload, MessageId, timestamp, header, Secret["whsec_".Length..]));
    }

    [Fact]
    public void Verify_MultipleVersionsInHeader_MatchesAny()
    {
        var timestamp = Now();
        var header = "v1,bm90LXRoZS1yaWdodC1vbmU= " + Sign(MessageId, timestamp, Payload, Secret);

        Assert.True(SvixSignature.Verify(Payload, MessageId, timestamp, header, Secret));
    }

    [Fact]
    public void Verify_TamperedPayload_ReturnsFalse()
    {
        var timestamp = Now();
        var header = Sign(MessageId, timestamp, Payload, Secret);

        Assert.False(SvixSignature.Verify(Payload + " ", MessageId, timestamp, header, Secret));
    }

    [Fact]
    public void Verify_DifferentMessageId_ReturnsFalse()
    {
        var timestamp = Now();
        var header = Sign(MessageId, timestamp, Payload, Secret);

        Assert.False(SvixSignature.Verify(Payload, "msg_other", timestamp, header, Secret));
    }

    [Fact]
    public void Verify_StaleTimestamp_ReturnsFalse()
    {
        var timestamp = DateTimeOffset.UtcNow.AddMinutes(-10).ToUnixTimeSeconds().ToString();
        var header = Sign(MessageId, timestamp, Payload, Secret);

        Assert.False(SvixSignature.Verify(Payload, MessageId, timestamp, header, Secret));
    }

    [Fact]
    public void Verify_FutureTimestampBeyondTolerance_ReturnsFalse()
    {
        var timestamp = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds().ToString();
        var header = Sign(MessageId, timestamp, Payload, Secret);

        Assert.False(SvixSignature.Verify(Payload, MessageId, timestamp, header, Secret));
    }

    [Fact]
    public void Verify_UnknownVersionOnly_ReturnsFalse()
    {
        var timestamp = Now();
        var header = Sign(MessageId, timestamp, Payload, Secret).Replace("v1,", "v2,");

        Assert.False(SvixSignature.Verify(Payload, MessageId, timestamp, header, Secret));
    }

    [Theory]
    [InlineData(null, "1", "v1,sig", Secret)]
    [InlineData(MessageId, null, "v1,sig", Secret)]
    [InlineData(MessageId, "1", null, Secret)]
    [InlineData(MessageId, "1", "v1,sig", null)]
    [InlineData(MessageId, "not-a-number", "v1,sig", Secret)]
    public void Verify_MissingOrMalformedInputs_ReturnsFalse(
        string? id, string? timestamp, string? signature, string? secret)
    {
        Assert.False(SvixSignature.Verify(Payload, id, timestamp, signature, secret));
    }

    private static string Now() => DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

    private static string Sign(string id, string timestamp, string payload, string secret)
    {
        var key = Convert.FromBase64String(secret["whsec_".Length..]);
        var content = Encoding.UTF8.GetBytes($"{id}.{timestamp}.{payload}");
        return "v1," + Convert.ToBase64String(HMACSHA256.HashData(key, content));
    }
}
