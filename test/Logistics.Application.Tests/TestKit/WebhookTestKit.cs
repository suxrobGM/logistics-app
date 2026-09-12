using System.Security.Cryptography;
using System.Text;

namespace Logistics.Application.Tests.TestKit;

internal static class WebhookTestKit
{
    /// <summary>Signs a payload the way the providers verify it: lowercase hex HMAC-SHA256.</summary>
    public static string ComputeHmacHex(string payload, string secret)
    {
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexStringLower(hash);
    }
}

/// <summary>Fails the test if a webhook path reaches out over HTTP.</summary>
internal sealed class NeverCalledHttpHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        throw new InvalidOperationException("Webhook tests must not perform HTTP calls.");
    }
}
