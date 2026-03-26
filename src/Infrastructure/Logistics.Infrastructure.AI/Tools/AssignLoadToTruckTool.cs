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
        var loadId = Guid.Parse(input["load_id"]!.GetValue<string>());
        var truckId = Guid.Parse(input["truck_id"]!.GetValue<string>());

        var result = await mediator.Send(new AssignLoadToTruckCommand { LoadId = loadId, TruckId = truckId }, ct);

        return result.IsSuccess
            ? JsonSerializer.Serialize(new { success = true, load_id = loadId, truck_id = truckId })
            : JsonSerializer.Serialize(new { success = false, error = result.Error });
    }
}
