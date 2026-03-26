using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Commands;
using MediatR;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class CreateTripTool(IMediator mediator) : IDispatchTool
{
    public string Name => "create_trip";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        if (!Guid.TryParse(input["truck_id"]?.GetValue<string>(), out var truckId))
            return JsonSerializer.Serialize(new { error = "Invalid or missing truck_id" });

        var loadIdNodes = input["load_ids"]?.AsArray();
        if (loadIdNodes is null || loadIdNodes.Count == 0)
            return JsonSerializer.Serialize(new { error = "Missing or empty load_ids" });

        var loadIds = new List<Guid>();
        foreach (var node in loadIdNodes)
        {
            if (!Guid.TryParse(node?.GetValue<string>(), out var id))
                return JsonSerializer.Serialize(new { error = $"Invalid load_id: {node}" });
            loadIds.Add(id);
        }

        var name = input["name"]?.GetValue<string>() ?? "AI-Generated Trip";

        var command = new CreateTripCommand
        {
            Name = name,
            TruckId = truckId,
            AttachedLoadIds = loadIds
        };

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? JsonSerializer.Serialize(new { success = true, trip_id = result.Value })
            : JsonSerializer.Serialize(new { success = false, error = result.Error });
    }
}
