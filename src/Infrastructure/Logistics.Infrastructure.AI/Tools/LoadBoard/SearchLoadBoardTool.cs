using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.Integrations.LoadBoard.Commands;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Logistics.Shared.Identity.Policies;
using Logistics.Mediator;

namespace Logistics.Infrastructure.AI.Tools.LoadBoard;

internal sealed class SearchLoadBoardTool(IMediator mediator)
    : AgentTool<SearchLoadBoardTool.Input>, IAgentToolMetadata
{
    private const int MaxResults = 20;

    internal sealed record Input
    {
        [Description("Origin city name")]
        public required string OriginCity { get; init; }

        [Description("Origin state code (e.g., 'TX', 'CA')")]
        public required string OriginState { get; init; }

        [Description("Search radius in miles from origin (default: 100)")]
        public int? RadiusMiles { get; init; }

        [Description("Optional destination state filter (e.g., 'AZ')")]
        public string? DestinationState { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "search_loadboard",
        "Search the connected load boards (DAT) for available loads leaving a city. Use this when trucks have capacity gaps to find revenue opportunities. market_rate_per_mile, when present, is the DAT RateView spot rate for the lane.")
    {
        RequiredFeature = TenantFeature.LoadBoard,
        RequiredPermission = Permission.Dispatch.View,
        Surfaces = AgentSurfaces.All
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.OriginCity) || string.IsNullOrWhiteSpace(input.OriginState))
            return ToolResult.Error("origin_city and origin_state must not be blank.");

        var command = new SearchLoadBoardCommand
        {
            Origin = new LoadBoardPlace { City = input.OriginCity, State = input.OriginState },
            OriginRadius = input.RadiusMiles ?? 100,
            Destination = input.DestinationState is { } destinationState
                ? new LoadBoardPlace { State = destinationState }
                : null,
            MaxResults = MaxResults
        };

        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess || result.Value is null)
            return ToolResult.Error(result.Error ?? "Load board search failed");

        var listings = result.Value.Listings.Select(l => new
        {
            // The persisted listing id, not the provider's - book_loadboard_load takes this one,
            // and external ids are not stable across searches on every provider.
            listing_id = l.Id,
            provider = l.ProviderType.ToString(),
            origin = l.OriginAddress.ToString(),
            destination = l.DestinationAddress.ToString(),
            rate_per_mile = l.RatePerMile,
            market_rate_per_mile = l.MarketRatePerMile,
            total_rate = l.TotalRate,
            currency = l.Currency,
            distance_miles = l.Distance,
            weight = l.Weight,
            equipment_type = l.EquipmentType?.GetDescription(),
            commodity = l.Commodity,
            pickup_date = l.PickupDateStart?.ToString("yyyy-MM-dd"),
            delivery_date = l.DeliveryDateStart?.ToString("yyyy-MM-dd"),
            broker_name = l.BrokerName,
            broker_mc_number = l.BrokerMcNumber,
            broker_credit_score = l.BrokerCreditScore,
            is_bookable = l.IsBookable
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
