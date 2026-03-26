using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Queries;
using MediatR;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class GetUnassignedLoadsTool(IMediator mediator) : IDispatchTool
{
    public string Name => "get_unassigned_loads";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        var result = await mediator.Send(new GetUnassignedLoadsQuery(), ct);

        if (!result.IsSuccess)
            return JsonSerializer.Serialize(new { error = result.Error });

        var items = result.Value?.ToList() ?? [];
        var loads = items.Select(l => new
        {
            id = l.Id,
            name = l.Name,
            type = l.Type.ToString(),
            origin = l.OriginAddress?.ToString(),
            destination = l.DestinationAddress?.ToString(),
            origin_lat = l.OriginLocation?.Latitude,
            origin_lng = l.OriginLocation?.Longitude,
            dest_lat = l.DestinationLocation?.Latitude,
            dest_lng = l.DestinationLocation?.Longitude,
            distance_km = l.Distance,
            delivery_cost = l.DeliveryCost,
            customer = l.Customer?.Name
        });

        return JsonSerializer.Serialize(new { loads, count = items.Count });
    }
}
