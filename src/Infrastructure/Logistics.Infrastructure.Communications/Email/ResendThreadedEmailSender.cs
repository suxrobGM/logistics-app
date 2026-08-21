using Logistics.Application.Abstractions.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resend;

namespace Logistics.Infrastructure.Communications.Email;

internal sealed class ResendThreadedEmailSender(
    IResend resend,
    IOptions<ResendOptions> options,
    ILogger<ResendThreadedEmailSender> logger) : IThreadedEmailSender
{
    public string ReplyDomain => options.Value.SenderDomain;

    public async Task<ThreadedEmailResult> SendAsync(ThreadedEmail email, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(email.To);
        ArgumentException.ThrowIfNullOrEmpty(email.Subject);
        ArgumentException.ThrowIfNullOrEmpty(email.HtmlBody);
        ArgumentException.ThrowIfNullOrEmpty(email.ReplyTo);

        try
        {
            var message = new EmailMessage
            {
                From = $"{options.Value.SenderName} <{options.Value.SenderEmail}>",
                Subject = email.Subject,
                HtmlBody = email.HtmlBody,
                ReplyTo = email.ReplyTo
            };
            message.To.Add(email.To);

            if (!string.IsNullOrEmpty(email.InReplyToMessageId))
            {
                message.Headers["In-Reply-To"] = email.InReplyToMessageId;
            }

            if (!string.IsNullOrEmpty(email.References))
            {
                message.Headers["References"] = email.References;
            }

            var response = await resend.EmailSendAsync(message, ct);
            logger.LogInformation(
                "Threaded email has been sent to {Mail}, subject: '{Subject}', provider id: {ProviderId}",
                email.To, email.Subject, response.Content);
            return new ThreadedEmailResult(true, response.Content.ToString());
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                "Could not send threaded email to {Mail}, subject: '{Subject}'. \nThrown exception: {Exception}",
                email.To, email.Subject, ex.ToString());
            return new ThreadedEmailResult(false, null);
        }
    }
}
