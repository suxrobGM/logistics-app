using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Negotiation.Commands;

/// <summary>
/// Sends a counter-offer email to the broker behind a load board listing. The recipient is read
/// from the listing server-side and is never part of this request.
/// </summary>
[RequiresFeature(TenantFeature.AIRateNegotiation)]
public class ProposeCounterOfferCommand : ICommand<Result<RateNegotiationDto>>
{
    public Guid ListingId { get; set; }

    public decimal ProposedTotalRate { get; set; }

    public decimal? ProposedRatePerMile { get; set; }

    /// <summary>The broker-facing paragraph. Sanitized and length-clamped before it is rendered.</summary>
    public string Message { get; set; } = "";

    /// <summary>Why the agent is countering, kept for the audit trail and never sent to the broker.</summary>
    public string Reasoning { get; set; } = "";

    /// <summary>Dispatch conversation to wake when the broker replies.</summary>
    public Guid? ConversationId { get; set; }

    /// <summary>The approved agent decision this send came from, if any.</summary>
    public Guid? DecisionId { get; set; }
}
