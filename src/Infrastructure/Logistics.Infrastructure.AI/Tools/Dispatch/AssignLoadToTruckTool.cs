using Logistics.Application.Abstractions.Agents;
using System.Text.Json.Nodes;
using MediatR;
using Logistics.Application.Modules.Operations.Loads.Commands;

namespace Logistics.Infrastructure.AI.Tools.Dispatch;

internal sealed class AssignLoadToTruckTool(IMediator mediator) : IAgentTool
{
    public string Name => "assign_load_to_truck";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        if (input.GetGuid("load_id") is not { } loadId)
            return ToolResult.Error("Invalid or missing load_id");

        if (input.GetGuid("truck_id") is not { } truckId)
            return ToolResult.Error("Invalid or missing truck_id");

        var result = await mediator.Send(new AssignLoadToTruckCommand { LoadId = loadId, TruckId = truckId }, ct);

        return ToolResult.Written(result, new { success = true, load_id = loadId, truck_id = truckId });
    }
}
