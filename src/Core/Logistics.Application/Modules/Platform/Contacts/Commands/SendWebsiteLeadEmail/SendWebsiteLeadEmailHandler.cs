using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Email;
using Logistics.Domain.Options;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Modules.Platform.Contacts.Commands;

internal sealed class SendWebsiteLeadEmailHandler(
    IThreadedEmailSender emailSender,
    IEmailTemplateService emailTemplateService,
    IOptions<LeadNotificationOptions> options) : IRequestHandler<SendWebsiteLeadEmailCommand, Result>
{
    public async Task<Result> Handle(SendWebsiteLeadEmailCommand req, CancellationToken ct)
    {
        var recipient = options.Value.Email;
        if (string.IsNullOrWhiteSpace(recipient))
        {
            return Result.Ok();
        }

        var body = await emailTemplateService.RenderAsync("WebsiteLead", req.Email);
        var result = await emailSender.SendAsync(new ThreadedEmail(recipient, req.Subject, body, req.ReplyTo), ct);

        // Throwing (rather than returning a failure) is what makes the background job retry the send.
        if (!result.Success)
        {
            throw new InvalidOperationException($"Failed to forward website lead '{req.Subject}' to {recipient}");
        }

        return Result.Ok();
    }
}
