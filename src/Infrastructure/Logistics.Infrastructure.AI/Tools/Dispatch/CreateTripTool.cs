using System.Text.Json.Nodes;
using MediatR;
using Logistics.Application.Modules.Operations.Trips.Commands;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class CreateTripTool(IMediator mediator) : IAIDispatchTool
{
    public string Name => "create_trip";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        if (input.GetGuid("truck_id") is not { } truckId)
            return ToolResult.Error("Invalid or missing truck_id");

        var loadIdNodes = input.GetArray("load_ids");
        if (loadIdNodes.Count == 0)
            return ToolResult.Error("Missing or empty load_ids");

        // One bad id fails the whole call rather than quietly building a shorter trip - the
        // dispatcher approving this needs the trip to be the one the agent described.
        var loadIds = new List<Guid>();
        foreach (var node in loadIdNodes)
        {
            if (!Guid.TryParse(node?.ToString(), out var id))
                return ToolResult.Error($"Invalid load_id: {node}");
            loadIds.Add(id);
        }

        var name = input.GetString("name") ?? "AI-Generated Trip";

        var command = new CreateTripCommand
        {
            Name = name,
            TruckId = truckId,
            AttachedLoadIds = loadIds
        };

        var result = await mediator.Send(command, ct);

        return ToolResult.Written(result, new { success = true, trip_id = result.Value });
    }
}
