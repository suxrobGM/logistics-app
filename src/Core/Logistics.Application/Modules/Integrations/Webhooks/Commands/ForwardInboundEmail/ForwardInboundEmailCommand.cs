using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Webhooks.Commands;

/// <summary>
/// Forwards a received email that no feature handles (anything but a negotiation reply) to the address in
/// <see cref="Domain.Options.OwnerNotificationOptions"/>, replying to the original sender. The provider keeps
/// received mail only in its dashboard, so without this nobody sees it.
/// </summary>
public class ForwardInboundEmailCommand : ICommand
{
    public required string ProviderEmailId { get; init; }
}
