using System.Net;
using Logistics.Application.Abstractions.Email;
using Logistics.Domain.Options;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Modules.Integrations.Webhooks.Commands;

internal sealed class ForwardInboundEmailHandler(
    IInboundEmailReader emailReader,
    IThreadedEmailSender emailSender,
    IOptions<OwnerNotificationOptions> options,
    ILogger<ForwardInboundEmailHandler> logger) : IRequestHandler<ForwardInboundEmailCommand, Result>
{
    public async Task<Result> Handle(ForwardInboundEmailCommand req, CancellationToken ct)
    {
        var recipient = options.Value.Email;
        if (string.IsNullOrWhiteSpace(recipient))
        {
            return Result.Ok();
        }

        var email = await emailReader.GetAsync(req.ProviderEmailId, ct);
        if (email is null)
        {
            return Result.Fail($"Received email {req.ProviderEmailId} could not be read yet");
        }

        // Mail from our own domain would come straight back through the inbound webhook and loop.
        var isFromOwnDomain = email.From.EndsWith($"@{emailSender.ReplyDomain}", StringComparison.OrdinalIgnoreCase);
        if (isFromOwnDomain)
        {
            logger.LogInformation("Not forwarding received email {EmailId} sent from our own domain", email.Id);
            return Result.Ok();
        }

        var subject = string.IsNullOrWhiteSpace(email.Subject) ? "(no subject)" : email.Subject;
        var forward = new ThreadedEmail(recipient, subject, BuildBody(email), ReplyTo: email.From);
        var result = await emailSender.SendAsync(forward, ct);

        return result.Success
            ? Result.Ok()
            : Result.Fail($"Received email {req.ProviderEmailId} could not be forwarded to {recipient}");
    }

    private static string BuildBody(InboundEmail email)
    {
        var from = WebUtility.HtmlEncode(email.From);
        var to = WebUtility.HtmlEncode(string.Join(", ", email.To));
        var header = $"""<p style="margin: 0 0 12px; font-size: 13px; color: #71717a;">From: {from}<br>To: {to}</p><hr>""";

        var content = string.IsNullOrWhiteSpace(email.HtmlBody)
            ? $"""<pre style="white-space: pre-wrap; font-family: inherit;">{WebUtility.HtmlEncode(email.TextBody ?? "")}</pre>"""
            : email.HtmlBody;

        return header + content;
    }
}
