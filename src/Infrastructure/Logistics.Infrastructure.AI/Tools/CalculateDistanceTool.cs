using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class CalculateDistanceTool : IDispatchTool
{
    public string Name => "calculate_distance";

    public Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        double? originLat = input["origin_lat"]?.GetValue<double>();
        double? originLng = input["origin_lng"]?.GetValue<double>();
        double? destLat = input["dest_lat"]?.GetValue<double>();
        double? destLng = input["dest_lng"]?.GetValue<double>();

        if (originLat is null || originLng is null || destLat is null || destLng is null)
            return Task.FromResult(JsonSerializer.Serialize(new { error = "Missing required coordinate parameters" }));

        var origin = new GeoPoint(originLng.Value, originLat.Value);
        var destination = new GeoPoint(destLng.Value, destLat.Value);
        var straightLineKm = origin.DistanceTo(destination) / 1000.0;
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
