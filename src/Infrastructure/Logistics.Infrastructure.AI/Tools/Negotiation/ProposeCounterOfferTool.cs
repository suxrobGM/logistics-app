using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.Integrations.Negotiation.Commands;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;
using MediatR;

namespace Logistics.Infrastructure.AI.Tools.Negotiation;

internal sealed class ProposeCounterOfferTool(IMediator mediator, IAgentRunContext runContext)
    : AgentTool<ProposeCounterOfferTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("The listing_id (GUID) returned by search_loadboard")]
        public required Guid ListingId { get; init; }

        [Description("The total rate to offer, at or above the floor from get_rate_floor")]
        public required decimal ProposedTotalRate { get; init; }

        [Description("One short professional paragraph for the broker. State the offer and one reason for it. No greeting, no signature - the template adds those.")]
        public required string Message { get; init; }

        [Description("Brief explanation for the dispatcher of why this offer makes sense. Never sent to the broker.")]
        public required string Reasoning { get; init; }

        [Description("Optional per-mile equivalent of the offer")]
        public decimal? ProposedRatePerMile { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "propose_counter_offer",
        "Email a counter-offer to the broker behind a load board listing. Call get_rate_floor first: the offer is rejected if it is below the floor, if no floor covers the lane, or if the round budget is spent. You write only the broker-facing paragraph - the address, the rate line and the rest of the email are filled in by the system.")
    {
        RequiredFeature = TenantFeature.AIRateNegotiation,
        RequiredPermission = Permission.Negotiation.Manage,
        DecisionType = AgentDecisionType.ProposeCounterOffer,
        DispatchAgent = true
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Message))
            return ToolResult.Error("message is blank - it is the paragraph the broker reads.");

        // The broker address is never a model input: the handler reads it off the listing.
        var command = new ProposeCounterOfferCommand
        {
            ListingId = input.ListingId,
            ProposedTotalRate = input.ProposedTotalRate,
            ProposedRatePerMile = input.ProposedRatePerMile,
            Message = input.Message,
            ConversationId = runContext.ConversationId,
            DecisionId = runContext.DecisionId
        };

        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess || result.Value is null)
            return ToolResult.WriteFailed(result);

        var negotiation = result.Value;
        return ToolResult.Ok(new
        {
            success = true,
            negotiation_id = negotiation.Id,
            listing_id = negotiation.LoadBoardListingId,
            reference = negotiation.Reference,
            status = negotiation.Status.ToString(),
            rounds_used = negotiation.RoundCount,
            max_rounds = negotiation.MaxRounds,
            offered_total_rate = negotiation.LatestCounterOffer?.Amount,
            expires_at = negotiation.ExpiresAt
        });
    }
}
