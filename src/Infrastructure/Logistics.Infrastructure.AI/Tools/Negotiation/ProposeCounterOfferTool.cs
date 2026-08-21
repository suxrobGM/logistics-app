using System.Text.Json.Nodes;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.Integrations.Negotiation.Commands;
using MediatR;

namespace Logistics.Infrastructure.AI.Tools.Negotiation;

internal sealed class ProposeCounterOfferTool(IMediator mediator, IAgentRunContext runContext) : IAgentTool
{
    public string Name => "propose_counter_offer";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        if (input.GetGuid("listing_id") is not { } listingId)
            return ToolResult.Error("Invalid or missing listing_id - use the listing_id returned by search_loadboard");

        if (input.GetDecimal("proposed_total_rate") is not { } proposedTotalRate)
            return ToolResult.Error("Invalid or missing proposed_total_rate");

        var message = input.GetString("message");
        if (string.IsNullOrWhiteSpace(message))
            return ToolResult.Error("message is required - it is the paragraph the broker reads");

        // The broker address is never a model input: the handler reads it off the listing.
        var command = new ProposeCounterOfferCommand
        {
            ListingId = listingId,
            ProposedTotalRate = proposedTotalRate,
            ProposedRatePerMile = input.GetDecimal("proposed_rate_per_mile"),
            Message = message,
            Reasoning = input.GetString("reasoning") ?? "",
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
