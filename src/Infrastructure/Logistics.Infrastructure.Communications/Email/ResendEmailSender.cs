using Logistics.Application.Contracts.Services.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resend;

namespace Logistics.Infrastructure.Communications.Email;

internal sealed class ResendEmailSender(
    IResend resend,
    IOptions<ResendOptions> options,
    ILogger<ResendEmailSender> logger) : IEmailSender
{
    public async Task<bool> SendEmailAsync(string recipient, string subject, string htmlBody)
    {
        ArgumentException.ThrowIfNullOrEmpty(recipient);
        ArgumentException.ThrowIfNullOrEmpty(subject);
        ArgumentException.ThrowIfNullOrEmpty(htmlBody);

        try
        {
            var message = new EmailMessage
            {
                From = $"{options.Value.SenderName} <{options.Value.SenderEmail}>",
                Subject = subject,
                HtmlBody = htmlBody
            };
            message.To.Add(recipient);

            await resend.EmailSendAsync(message);
            logger.LogInformation("Email has been sent to {Mail}, subject: '{Subject}'", recipient, subject);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                "Could not send email to {Mail}, subject: '{Subject}'. \nThrown exception: {Exception}",
                recipient, subject, ex.ToString());
            return false;
        }
    }
}
