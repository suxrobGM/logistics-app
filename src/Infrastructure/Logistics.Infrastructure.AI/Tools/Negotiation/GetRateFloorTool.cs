using System.ComponentModel;
using Logistics.Application.Abstractions.AI;
using Logistics.Application.Modules.AI.Negotiation.Queries;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Identity.Policies;

namespace Logistics.Infrastructure.AI.Tools.Negotiation;

internal sealed class GetRateFloorTool(IMediator mediator, ITenantUnitOfWork tenantUow)
    : AgentTool<GetRateFloorTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("The listing_id (GUID) returned by search_loadboard")]
        public required Guid ListingId { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "get_rate_floor",
        "The minimum rate this carrier accepts on a listing's lane, and how the listing compares to it. Returns has_floor, the floor per distance unit (_mi or _km) and total, below_floor, the gap, whether the listing has a broker email, and any negotiation already running on it. Call this before proposing a counter-offer - without a floor there is no basis to negotiate.")
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

        var unit = tenantUow.GetCurrentTenant().Settings.DistanceUnit;
        var suffix = unit.Abbreviation();
        return ToolResult.Ok(new Dictionary<string, object?>
        {
            ["listing_id"] = context.ListingId,
            ["has_floor"] = floor.HasFloor,
            ["floor_source"] = floor.Source.ToString(),
            [$"min_rate_per_{suffix}"] = DispatchUnits.RateFromPerKm(floor.MinRatePerKm, unit),
            ["min_total_rate"] = floor.MinTotalRate,
            ["effective_floor_total"] = floor.EffectiveFloorTotal,
            ["below_floor"] = floor.ListingBelowFloor,
            [$"gap_per_{suffix}"] = DispatchUnits.RateFromPerKm(floor.GapPerKm, unit),
            ["listing_total_rate"] = context.ListingTotalRate,
            [$"listing_rate_per_{suffix}"] = DispatchUnits.RateFromPerKm(context.ListingRatePerKm, unit),
            [$"distance_{suffix}"] = context.DistanceMeters is { } meters ? DispatchUnits.FromMeters(meters, unit) : null,
            ["currency"] = context.Currency,
            ["broker_email_available"] = context.BrokerEmailAvailable,
            ["has_active_negotiation"] = context.ActiveNegotiationId is not null,
            ["negotiation_id"] = context.ActiveNegotiationId,
            ["rounds_used"] = context.RoundCount,
            ["max_rounds"] = context.MaxRounds
        });
    }
}
