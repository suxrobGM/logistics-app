namespace Logistics.Application.Abstractions.Email;

/// <summary>
/// Verifies the signature on an inbound-email webhook. The signature is the only authentication an
/// anonymous webhook route has, so an implementation must fail closed.
/// </summary>
public interface IInboundEmailWebhookVerifier
{
    bool Verify(string rawBody, string? messageId, string? timestamp, string? signature);
}
