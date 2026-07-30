using System.Text.Json.Nodes;
using Logistics.Application.Modules.Integrations.LoadBoard.Commands;
using Logistics.Domain.Primitives.ValueObjects;
using MediatR;

namespace Logistics.Infrastructure.AI.Tools.LoadBoard;

internal sealed class SearchLoadBoardTool(IMediator mediator) : IAgentTool
{
    private const int MaxResults = 20;

    public string Name => "search_loadboard";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        var originCity = input.GetString("origin_city");
        var originState = input.GetString("origin_state");

        if (string.IsNullOrWhiteSpace(originCity) || string.IsNullOrWhiteSpace(originState))
            return ToolResult.Error("Both origin_city and origin_state are required");

        var command = new SearchLoadBoardCommand
        {
            OriginAddress = CityState(originCity, originState),
            OriginRadius = input.GetInt("radius_miles") ?? 100,
            DestinationAddress = input.GetString("destination_state") is { } destinationState
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
