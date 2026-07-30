using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Infrastructure.AI.Tools.Dispatch;

internal sealed class CalculateDistanceTool : IAgentTool
{
    public string Name => "calculate_distance";

    public Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        double? originLat = input.GetDouble("origin_lat");
        double? originLng = input.GetDouble("origin_lng");
        double? destLat = input.GetDouble("dest_lat");
        double? destLng = input.GetDouble("dest_lng");

        if (originLat is null || originLng is null || destLat is null || destLng is null)
            return Task.FromResult(ToolResult.Error("Missing required coordinate parameters"));

        var origin = new GeoPoint(originLng.Value, originLat.Value);
        var destination = new GeoPoint(destLng.Value, destLat.Value);
        var straightLineKm = DispatchUnits.MetersToKm(origin.DistanceTo(destination));
        var drivingDistanceKm = straightLineKm * 1.3;
        var estimatedMinutes = (int)(drivingDistanceKm / 80.0 * 60);

        return Task.FromResult(JsonSerializer.Serialize(new
        {
            straight_line_km = Math.Round(straightLineKm, 1),
            estimated_driving_km = Math.Round(drivingDistanceKm, 1),
            estimated_minutes = estimatedMinutes
        }));
    }
}
