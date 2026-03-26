using System.Text.Json;
using System.Text.Json.Nodes;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class OptimizeTripStopsTool : IDispatchTool
{
    public string Name => "optimize_trip_stops";

    public Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        var loadIds = input["load_ids"]!.AsArray()
            .Select(n => Guid.Parse(n!.GetValue<string>())).ToList();

        return Task.FromResult(JsonSerializer.Serialize(new
        {
            optimized = true,
            load_ids = loadIds,
            message = "Stops will be optimized when the trip is created"
        }));
    }
}
