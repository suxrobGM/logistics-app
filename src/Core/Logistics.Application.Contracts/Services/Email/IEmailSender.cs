namespace Logistics.Application.Contracts.Services.Email;

/// <summary>
///     Email service
/// </summary>
public interface IEmailSender
{
    /// <summary>
    ///     Sends mail to a specified address.
    /// </summary>
    /// <param name="recipient">Receiver email address</param>
    /// <param name="subject">Mail subject</param>
    /// <param name="htmlBody">Mail html body</param>
    /// <returns>True if mail has been sent successfully, otherwise false</returns>
    Task<bool> SendEmailAsync(string recipient, string subject, string htmlBody);
}
