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

        try
        {
            var headers = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(email.MessageId))
            {
                headers["Message-ID"] = email.MessageId;
            }

            if (!string.IsNullOrEmpty(email.InReplyToMessageId))
            {
                headers["In-Reply-To"] = email.InReplyToMessageId;
            }

            if (!string.IsNullOrEmpty(email.References))
            {
                headers["References"] = email.References;
            }

            var message = new EmailMessage
            {
                From = $"{options.Value.SenderName} <{options.Value.SenderEmail}>",
                Subject = email.Subject,
                HtmlBody = email.HtmlBody,
                ReplyTo = email.ReplyTo is null ? null : (EmailAddressList)email.ReplyTo,
                Headers = headers
            };

            message.To.Add(email.To);

            if (email.Attachments is { Count: > 0 })
            {
                message.Attachments = email.Attachments
                    .Select(a => new EmailAttachment { Filename = a.FileName, Content = a.Content, ContentType = a.ContentType })
                    .ToList();
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
