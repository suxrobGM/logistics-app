using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.Integrations.LoadBoard.Commands;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Identity.Policies;
using MediatR;

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
        "Search load boards (DAT, Truckstop, 123Loadboard) for available loads matching criteria. Use this when trucks have capacity gaps to find revenue opportunities.")
    {
        RequiredFeature = TenantFeature.LoadBoard,
        RequiredPermission = Permission.Dispatch.View,
        DispatchAgent = true
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.OriginCity) || string.IsNullOrWhiteSpace(input.OriginState))
            return ToolResult.Error("Both origin_city and origin_state are required");

        var command = new SearchLoadBoardCommand
        {
            OriginAddress = CityState(input.OriginCity, input.OriginState),
            OriginRadius = input.RadiusMiles ?? 100,
            DestinationAddress = input.DestinationState is { } destinationState
                ? CityState(city: "", destinationState)
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
            total_rate = l.TotalRate,
            currency = l.Currency,
            distance_miles = l.Distance,
            weight = l.Weight,
            equipment_type = l.EquipmentType,
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

    /// <summary>
    /// The command wants a full <see cref="Address"/>, but city and state are all a model
    /// realistically has for a radius search. The remaining fields are required by the type, not
    /// by the providers, which match on city, state, and radius.
    /// </summary>
    private static Address CityState(string city, string state) => new()
    {
        Line1 = "",
        City = city,
        State = state,
        ZipCode = "",
        Country = "US"
    };
}
