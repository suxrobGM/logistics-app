using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Services;

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
        ArgumentException.ThrowIfNullOrEmpty(_options.UserName);
        ArgumentException.ThrowIfNullOrEmpty(_options.Password);
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
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Host = _options.Host!;
            smtpClient.EnableSsl = true;
            smtpClient.Timeout = 5000;
            smtpClient.Credentials = new NetworkCredential(_options.UserName, _options.Password);

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Email has been sent to {Mail}, subject: \'{Subject}\'", recipient, subject);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                "Could not send email to {Mail}, subject: \'{Subject}\'. \nThrown exception: {Exception}", 
                recipient, subject, ex.ToString());
            return false;
        }
    }
}
