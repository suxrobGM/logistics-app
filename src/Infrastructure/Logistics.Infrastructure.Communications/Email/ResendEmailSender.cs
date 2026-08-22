using Logistics.Application.Abstractions.Email;

namespace Logistics.Infrastructure.Communications.Email;

/// <summary>
/// A one-off email is a threaded email with no reply address and no threading headers, so this
/// delegates rather than assembling a second Resend message of its own.
/// </summary>
internal sealed class ResendEmailSender(IThreadedEmailSender threadedEmailSender) : IEmailSender
{
    public async Task<bool> SendEmailAsync(string recipient, string subject, string htmlBody)
    {
        var result = await threadedEmailSender.SendAsync(
            new ThreadedEmail(To: recipient, Subject: subject, HtmlBody: htmlBody, ReplyTo: null));

        return result.Success;
    }
}
