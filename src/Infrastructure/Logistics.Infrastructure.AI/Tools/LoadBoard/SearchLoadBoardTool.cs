using System.ComponentModel;
using Logistics.Application.Abstractions.AI;
using Logistics.Application.Modules.Integrations.LoadBoard.Commands;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.AI.Tools.LoadBoard;

internal sealed class SearchLoadBoardTool(IMediator mediator, ITenantUnitOfWork tenantUow)
    : AgentTool<SearchLoadBoardTool.Input>, IAgentToolMetadata
{
    private const int MaxResults = 20;

    internal sealed record Input
    {
        [Description("Origin city name")]
        public required string OriginCity { get; init; }

        [Description("Origin state code (e.g., 'TX', 'CA')")]
        public required string OriginState { get; init; }

        [Description("Search radius from origin, in the company's distance unit (default: 100)")]
        public int? Radius { get; init; }

        [Description("Optional destination state filter (e.g., 'AZ')")]
        public string? DestinationState { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "search_loadboard",
        "Search the connected load boards (DAT) for available loads leaving a city. Use this when trucks have capacity gaps to find revenue opportunities. Rate and distance keys end in the company's unit (`_mi` or `_km`). market_rate_per_{unit}, when present, is the DAT RateView spot rate for the lane. est_margin_per_{unit}, when present, is the rate minus the fleet's operating cost per distance unit (no deadhead or driver pay); a negative one is a below-cost load.")
    {
        RequiredFeature = TenantFeature.LoadBoard,
        RequiredPermission = Permission.Dispatch.View,
        Surfaces = AgentSurfaces.All
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.OriginCity) || string.IsNullOrWhiteSpace(input.OriginState))
            return ToolResult.Error("origin_city and origin_state must not be blank.");

        var unit = tenantUow.GetCurrentTenant().Settings.DistanceUnit;
        var command = new SearchLoadBoardCommand
        {
            Origin = new LoadBoardPlace { City = input.OriginCity, State = input.OriginState },
            OriginRadiusMeters = (int)Math.Round(unit.ToMeters(input.Radius ?? 100)),
            Destination = input.DestinationState is { } destinationState
                ? new LoadBoardPlace { State = destinationState }
                : null,
            MaxResults = MaxResults
        };

        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess || result.Value is null)
            return ToolResult.Error(result.Error ?? "Load board search failed");

        var suffix = unit.Abbreviation();
        var listings = result.Value.Listings.Select(l => new Dictionary<string, object?>
        {
            // The persisted listing id, not the provider's - book_loadboard_load takes this one,
            // and external ids are not stable across searches on every provider.
            ["listing_id"] = l.Id,
            ["provider"] = l.ProviderType.ToString(),
            ["origin"] = l.OriginAddress.ToString(),
            ["destination"] = l.DestinationAddress.ToString(),
            [$"rate_per_{suffix}"] = DispatchUnits.RateFromPerKm(l.RatePerKm, unit),
            [$"market_rate_per_{suffix}"] = DispatchUnits.RateFromPerKm(l.MarketRatePerKm, unit),
            [$"est_margin_per_{suffix}"] = DispatchUnits.RateFromPerKm(l.EstimatedMarginPerKm, unit),
            ["total_rate"] = l.TotalRate,
            ["currency"] = l.Currency,
            [$"distance_{suffix}"] = l.Distance is { } meters ? DispatchUnits.FromMeters(meters, unit) : null,
            ["weight_kg"] = l.WeightKg,
            ["equipment_type"] = l.EquipmentType?.GetDescription(),
            ["commodity"] = l.Commodity,
            ["pickup_date"] = l.PickupDateStart?.ToString("yyyy-MM-dd"),
            ["delivery_date"] = l.DeliveryDateStart?.ToString("yyyy-MM-dd"),
            ["broker_name"] = l.BrokerName,
            ["broker_mc_number"] = l.BrokerMcNumber,
            ["broker_credit_score"] = l.BrokerCreditScore,
            ["is_bookable"] = l.IsBookable
        }).ToList();

        return ToolResult.Ok(new
        {
            listings,
            count = listings.Count,
            total = result.Value.TotalCount,
            truncated = result.Value.TotalCount > listings.Count,
            // A per-provider failure does not fail the search, so surface it - otherwise the agent
            // concludes there is no freight when a board simply did not answer.
            provider_errors = result.Value.Errors?
                .Select(e => new { provider = e.Key.ToString(), error = e.Value })
                .ToList()
        });
    }
}
