using System.ComponentModel;
using System.Text.Json;
using Logistics.Application.Abstractions.Agents;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Identity.Policies;

namespace Logistics.Infrastructure.AI.Tools.Dispatch;

internal sealed class CalculateDistanceTool : AgentTool<CalculateDistanceTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("Origin latitude")]
        public required double OriginLat { get; init; }

        [Description("Origin longitude")]
        public required double OriginLng { get; init; }

        [Description("Destination latitude")]
        public required double DestLat { get; init; }

        [Description("Destination longitude")]
        public required double DestLng { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "calculate_distance",
        "Calculate the driving distance and estimated duration between two geographic points.")
    {
        RequiredPermission = Permission.Dispatch.View,
        DispatchAgent = true
    };

    protected override Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var origin = new GeoPoint(input.OriginLng, input.OriginLat);
        var destination = new GeoPoint(input.DestLng, input.DestLat);
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
