using Logistics.Application.Abstractions.Email;
using Microsoft.Extensions.Logging;
using Resend;

namespace Logistics.Infrastructure.Communications.Email;

/// <summary>
/// Reads a received email through Resend's Received Emails API.
/// </summary>
internal sealed class ResendInboundEmailReader(
    IResend resend,
    ILogger<ResendInboundEmailReader> logger) : IInboundEmailReader
{
    public async Task<InboundEmail?> GetAsync(string providerEmailId, CancellationToken ct = default)
    {
        if (!Guid.TryParse(providerEmailId, out var emailId))
        {
            logger.LogWarning("Received email id {EmailId} is not a Resend id", providerEmailId);
            return null;
        }

        ReceivedEmail email;
        try
        {
            email = (await resend.ReceivedEmailRetrieveAsync(emailId, ct)).Content;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Fetching received email {EmailId} from Resend failed", providerEmailId);
            return null;
        }

        if (string.IsNullOrWhiteSpace(email.From?.Email))
        {
            logger.LogWarning("Resend returned an unusable body for received email {EmailId}", providerEmailId);
            return null;
        }

        return new InboundEmail(
            email.Id.ToString(),
            email.From.Email,
            email.To?.Select(to => to.Email).ToList() ?? [],
            email.Subject,
            email.TextBody,
            email.HtmlBody,
            email.MessageId);
    }
}
