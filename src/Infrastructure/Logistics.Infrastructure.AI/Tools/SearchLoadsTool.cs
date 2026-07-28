using System.Text.Json;
using System.Text.Json.Nodes;
using MediatR;
using Logistics.Application.Modules.Operations.Loads.Queries;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class SearchLoadsTool(IMediator mediator) : IAIDispatchTool
{
    private const int MaxResults = 20;

    public string Name => "search_loads";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        var query = new GetLoadsQuery
        {
            Search = input.GetString("search"),
            Statuses = input.GetEnumArray<LoadStatus>("statuses"),
            Types = input.GetEnumArray<LoadType>("types"),
            CustomerId = input.GetGuid("customer_id"),
            TruckId = input.GetGuid("truck_id"),
            StartDate = input.GetDate("start_date"),
            EndDate = input.GetDate("end_date"),
            OrderBy = "-CreatedAt",
            Page = input.GetInt("page") ?? 1,
            PageSize = MaxResults
        };

        var result = await mediator.Send(query, ct);

        if (!result.IsSuccess)
            return JsonSerializer.Serialize(new { error = result.Error });

        var items = result.Value?.ToList() ?? [];
        var loads = items.Select(l => new
        {
            id = l.Id,
            number = l.Number,
            name = l.Name,
            status = l.Status.ToString(),
            type = l.Type.ToString(),
            origin = l.OriginAddress.ToString(),
            destination = l.DestinationAddress.ToString(),
            delivery_cost = l.DeliveryCost,
            customer = l.Customer?.Name,
            customer_id = l.Customer?.Id,
            truck_number = l.AssignedTruckNumber,
            delivered_at = l.DeliveredAt?.ToString("yyyy-MM-dd"),
            created_at = l.CreatedAt.ToString("yyyy-MM-dd")
        });

        return JsonSerializer.Serialize(new
        {
            loads,
            count = items.Count,
            total = result.TotalItems,
            truncated = result.TotalItems > items.Count
        });
    }
}
