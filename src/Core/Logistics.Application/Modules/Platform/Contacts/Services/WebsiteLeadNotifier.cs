using Logistics.Application.Abstractions.Email;
using Logistics.Application.Abstractions.Email.Models;
using Logistics.Domain.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Modules.Platform.Contacts.Services;

internal sealed class WebsiteLeadNotifier(
    IThreadedEmailSender emailSender,
    IEmailTemplateService emailTemplateService,
    IOptions<LeadNotificationOptions> options,
    ILogger<WebsiteLeadNotifier> logger) : IWebsiteLeadNotifier
{
    public async Task NotifyAsync(
        string subject,
        string replyTo,
        WebsiteLeadEmailModel model,
        CancellationToken ct = default)
    {
        var recipient = options.Value.Email;
        if (string.IsNullOrWhiteSpace(recipient))
        {
            return;
        }

        try
        {
            var body = await emailTemplateService.RenderAsync("WebsiteLead", model);
            var result = await emailSender.SendAsync(new ThreadedEmail(recipient, subject, body, replyTo), ct);
            if (!result.Success)
            {
                logger.LogWarning("Failed to forward website lead '{Subject}' to {Recipient}", subject, recipient);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to forward website lead '{Subject}' to {Recipient}", subject, recipient);
        }
    }
}
