using System.Text.Json.Nodes;
using Logistics.Application.Modules.Integrations.Negotiation.Queries;
using MediatR;

namespace Logistics.Infrastructure.AI.Tools.Negotiation;

internal sealed class GetRateFloorTool(IMediator mediator) : IAgentTool
{
    public string Name => "get_rate_floor";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        if (input.GetGuid("listing_id") is not { } listingId)
            return ToolResult.Error("Invalid or missing listing_id - use the listing_id returned by search_loadboard");

        var result = await mediator.Send(new GetRateFloorContextQuery { ListingId = listingId }, ct);

        if (!result.IsSuccess || result.Value is null)
            return ToolResult.Error(result.Error ?? "Could not resolve a rate floor for this listing");

        var context = result.Value;
        var floor = context.Floor;

        return ToolResult.Ok(new
        {
            listing_id = context.ListingId,
            has_floor = floor.HasFloor,
            floor_source = floor.Source.ToString(),
            min_rate_per_mile = floor.MinRatePerMile,
            min_total_rate = floor.MinTotalRate?.Amount,
            effective_floor_total = floor.EffectiveFloorTotal,
            below_floor = floor.ListingBelowFloor,
            gap_per_mile = floor.GapPerMile,
            listing_total_rate = context.ListingTotalRate,
            listing_rate_per_mile = context.ListingRatePerMile,
            distance_miles = context.DistanceMiles,
            currency = context.Currency,
            broker_email_available = context.BrokerEmailAvailable,
            has_active_negotiation = context.ActiveNegotiationId is not null,
            negotiation_id = context.ActiveNegotiationId,
            rounds_used = context.RoundCount,
            max_rounds = context.MaxRounds
        });
    }
}
