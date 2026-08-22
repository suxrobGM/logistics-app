using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.Integrations.Negotiation.Queries;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;
using MediatR;

namespace Logistics.Infrastructure.AI.Tools.Negotiation;

internal sealed class GetRateFloorTool(IMediator mediator)
    : AgentTool<GetRateFloorTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("The listing_id (GUID) returned by search_loadboard")]
        public required Guid ListingId { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "get_rate_floor",
        "The minimum rate this carrier accepts on a listing's lane, and how the listing compares to it. Returns has_floor, the floor per mile and total, below_floor, the gap, whether the listing has a broker email, and any negotiation already running on it. Call this before proposing a counter-offer - without a floor there is no basis to negotiate.")
    {
        RequiredFeature = TenantFeature.AIRateNegotiation,
        RequiredPermission = Permission.Negotiation.View,
        Surfaces = AgentSurfaces.All
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var result = await mediator.Send(new GetRateFloorContextQuery { ListingId = input.ListingId }, ct);

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
