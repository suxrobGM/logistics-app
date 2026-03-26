using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Commands;
using MediatR;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class AssignLoadToTruckTool(IMediator mediator) : IDispatchTool
{
    public string Name => "assign_load_to_truck";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        if (!Guid.TryParse(input["load_id"]?.GetValue<string>(), out var loadId))
            return JsonSerializer.Serialize(new { error = "Invalid or missing load_id" });

        if (!Guid.TryParse(input["truck_id"]?.GetValue<string>(), out var truckId))
            return JsonSerializer.Serialize(new { error = "Invalid or missing truck_id" });

        var result = await mediator.Send(new AssignLoadToTruckCommand { LoadId = loadId, TruckId = truckId }, ct);

        return result.IsSuccess
            ? JsonSerializer.Serialize(new { success = true, load_id = loadId, truck_id = truckId })
            : JsonSerializer.Serialize(new { success = false, error = result.Error });
    }
}
