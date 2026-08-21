using Logistics.Application.Abstractions.Email;
using Logistics.Infrastructure.Integrations.Common;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Communications.Email;

/// <summary>Resend signs webhooks with Svix.</summary>
internal sealed class ResendWebhookVerifier(IOptions<ResendOptions> options) : IInboundEmailWebhookVerifier
{
    public bool Verify(string rawBody, string? messageId, string? timestamp, string? signature) =>
        WebhookSignature.VerifySvix(rawBody, messageId, timestamp, signature, options.Value.WebhookSecret);
}
