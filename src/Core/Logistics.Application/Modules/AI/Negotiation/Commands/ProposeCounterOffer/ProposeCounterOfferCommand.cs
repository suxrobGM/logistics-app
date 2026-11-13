using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.Negotiation.Commands;

/// <summary>Sends a counter-offer email to the broker of a load board listing, opening a negotiation thread when none is open. The broker's address is read from the listing on the server, never taken from the request.</summary>
[RequiresFeature(TenantFeature.AINegotiation)]
public class ProposeCounterOfferCommand : ICommand<Result<RateNegotiationDto>>
{
    public Guid ListingId { get; set; }

    public decimal ProposedTotalRate { get; set; }

    public decimal? ProposedRatePerKm { get; set; }

    /// <summary>The broker-facing paragraph. Sanitized and length-clamped before it is rendered.</summary>
    public string Message { get; set; } = "";

    /// <summary>Dispatch conversation to wake when the broker replies.</summary>
    public Guid? ConversationId { get; set; }

    /// <summary>The approved agent decision this send came from, if any.</summary>
    public Guid? DecisionId { get; set; }
}
