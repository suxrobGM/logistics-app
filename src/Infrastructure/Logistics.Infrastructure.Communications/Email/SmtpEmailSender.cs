using System.Net;
using System.Net.Mail;
using Logistics.Application.Contracts.Services.Email;
using Logistics.Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Communications.Email;

internal sealed class SmtpEmailSender : IEmailSender
{
    private readonly ILogger<SmtpEmailSender> logger;
    private readonly SmtpOptions options;

    public SmtpEmailSender(
        IOptions<SmtpOptions> options,
        ILogger<SmtpEmailSender> logger)
    {
        this.options = options.Value ?? throw new ArgumentNullException(nameof(options));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ArgumentException.ThrowIfNullOrEmpty(this.options.SenderName);
        ArgumentException.ThrowIfNullOrEmpty(this.options.SenderEmail);
        ArgumentException.ThrowIfNullOrEmpty(this.options.Host);
    }

    public async Task<bool> SendEmailAsync(string recipient, string subject, string htmlBody)
    {
        ArgumentException.ThrowIfNullOrEmpty(recipient);
        ArgumentException.ThrowIfNullOrEmpty(subject);
        ArgumentException.ThrowIfNullOrEmpty(htmlBody);

        try
        {
            var from = new MailAddress(options.SenderEmail!, options.SenderName);
            using var mailMessage = new MailMessage(from, new MailAddress(recipient));
            mailMessage.Subject = subject;
            mailMessage.Body = htmlBody;
            mailMessage.IsBodyHtml = true;
            mailMessage.Priority = MailPriority.High;
            mailMessage.DeliveryNotificationOptions = DeliveryNotificationOptions.Delay |
                                                      DeliveryNotificationOptions.OnFailure |
                                                      DeliveryNotificationOptions.OnSuccess;

            using var smtpClient = new SmtpClient(options.Host, options.Port);
            smtpClient.Port = options.Port;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.Host = options.Host!;
            smtpClient.EnableSsl = options.EnableSsl;
            smtpClient.Timeout = 5000;

            // Only set credentials if provided (for local testing without auth)
            if (!string.IsNullOrEmpty(options.UserName) && !string.IsNullOrEmpty(options.Password))
            {
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(options.UserName, options.Password);
            }

            await smtpClient.SendMailAsync(mailMessage);
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
