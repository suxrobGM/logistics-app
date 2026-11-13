using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.Negotiation.Commands;

/// <summary>Closes a rate negotiation and revokes its reply-to address, so later broker emails are no longer routed to the thread.</summary>
[RequiresFeature(TenantFeature.AINegotiation)]
public class CloseNegotiationCommand : ICommand<Result>
{
    public Guid Id { get; set; }

    public string? Reason { get; set; }

    /// <summary>Close as declined rather than simply closed.</summary>
    public bool Declined { get; set; }
}
