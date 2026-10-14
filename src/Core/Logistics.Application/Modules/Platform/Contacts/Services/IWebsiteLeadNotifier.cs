using Logistics.Application.Abstractions.Email.Models;

namespace Logistics.Application.Modules.Platform.Contacts.Services;

/// <summary>
/// Forwards a website demo request or contact message to the address in
/// <see cref="Domain.Options.LeadNotificationOptions"/>, replying to the sender. Never throws: the
/// submission is already stored, so a failed send must not fail the visitor's request.
/// </summary>
public interface IWebsiteLeadNotifier : IApplicationService
{
    Task NotifyAsync(string subject, string replyTo, WebsiteLeadEmailModel model, CancellationToken ct = default);
}
