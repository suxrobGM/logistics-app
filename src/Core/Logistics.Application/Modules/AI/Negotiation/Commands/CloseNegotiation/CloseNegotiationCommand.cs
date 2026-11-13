using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.Negotiation.Commands;

/// <summary>
/// Ends a negotiation thread and revokes its reply address, so later broker mail to that address
/// is dropped instead of reopening the conversation.
/// </summary>
[RequiresFeature(TenantFeature.AINegotiation)]
public class CloseNegotiationCommand : ICommand<Result>
{
    public Guid Id { get; set; }

    public string? Reason { get; set; }

    /// <summary>Close as declined rather than simply closed.</summary>
    public bool Declined { get; set; }
}
