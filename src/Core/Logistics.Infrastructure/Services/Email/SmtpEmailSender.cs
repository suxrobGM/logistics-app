using System.Net;
using System.Net.Mail;

using Logistics.Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Services.Email;

internal sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(
        IOptions<SmtpOptions> options,
        ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ArgumentException.ThrowIfNullOrEmpty(_options.SenderName);
        ArgumentException.ThrowIfNullOrEmpty(_options.SenderEmail);
        ArgumentException.ThrowIfNullOrEmpty(_options.Host);
    }

    public async Task<bool> SendEmailAsync(string recipient, string subject, string htmlBody)
    {
        ArgumentException.ThrowIfNullOrEmpty(recipient);
        ArgumentException.ThrowIfNullOrEmpty(subject);
        ArgumentException.ThrowIfNullOrEmpty(htmlBody);

        try
        {
            var from = new MailAddress(_options.SenderEmail!, _options.SenderName);
            using var mailMessage = new MailMessage(from, new MailAddress(recipient));
            mailMessage.Subject = subject;
            mailMessage.Body = htmlBody;
            mailMessage.IsBodyHtml = true;
            mailMessage.Priority = MailPriority.High;
            mailMessage.DeliveryNotificationOptions = DeliveryNotificationOptions.Delay |
                                                      DeliveryNotificationOptions.OnFailure |
                                                      DeliveryNotificationOptions.OnSuccess;

            using var smtpClient = new SmtpClient(_options.Host, _options.Port);
            smtpClient.Port = _options.Port;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.Host = _options.Host!;
            smtpClient.EnableSsl = _options.EnableSsl;
            smtpClient.Timeout = 5000;

            // Only set credentials if provided (for local testing without auth)
            if (!string.IsNullOrEmpty(_options.UserName) && !string.IsNullOrEmpty(_options.Password))
            {
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(_options.UserName, _options.Password);
            }

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Email has been sent to {Mail}, subject: '{Subject}'", recipient, subject);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                "Could not send email to {Mail}, subject: '{Subject}'. \nThrown exception: {Exception}",
                recipient, subject, ex.ToString());
            return false;
        }
    }
}
