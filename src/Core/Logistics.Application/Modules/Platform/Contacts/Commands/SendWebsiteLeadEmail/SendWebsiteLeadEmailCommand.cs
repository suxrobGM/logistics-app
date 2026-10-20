using Logistics.Application.Abstractions.Email.Models;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Contacts.Commands;

/// <summary>
/// Forwards a website demo request or contact message to the address in
/// <see cref="Domain.Options.OwnerNotificationOptions"/>, replying to the sender. Enqueued after the
/// submission is stored, so the visitor never waits on the email provider.
/// </summary>
public class SendWebsiteLeadEmailCommand : ICommand
{
    public required string Subject { get; init; }
    public required string ReplyTo { get; init; }
    public required WebsiteLeadEmailModel Email { get; init; }
}
