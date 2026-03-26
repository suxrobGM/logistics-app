using System.Text.Json;
using System.Text.Json.Nodes;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class OptimizeTripStopsTool : IDispatchTool
{
    public string Name => "optimize_trip_stops";

    public Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        var loadIdNodes = input["load_ids"]?.AsArray();
        if (loadIdNodes is null || loadIdNodes.Count == 0)
            return Task.FromResult(JsonSerializer.Serialize(new { error = "Missing or empty load_ids" }));

        var loadIds = new List<Guid>();
        foreach (var node in loadIdNodes)
        {
            if (!Guid.TryParse(node?.GetValue<string>(), out var id))
                return Task.FromResult(JsonSerializer.Serialize(new { error = $"Invalid load_id: {node}" }));
            loadIds.Add(id);
        }

        return Task.FromResult(JsonSerializer.Serialize(new
        {
            optimized = false,
            load_ids = loadIds,
            message = "Stop optimization is not yet implemented. Stops will be used in the order provided."
        }));
    }
}
