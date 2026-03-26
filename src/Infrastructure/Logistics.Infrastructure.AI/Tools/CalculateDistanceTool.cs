using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class CalculateDistanceTool : IDispatchTool
{
    public string Name => "calculate_distance";

    public Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        var originLat = input["origin_lat"]!.GetValue<double>();
        var originLng = input["origin_lng"]!.GetValue<double>();
        var destLat = input["dest_lat"]!.GetValue<double>();
        var destLng = input["dest_lng"]!.GetValue<double>();

        var origin = new GeoPoint(originLng, originLat);
        var destination = new GeoPoint(destLng, destLat);
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
