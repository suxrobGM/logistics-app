using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Identity.Policies;

namespace Logistics.Infrastructure.AI.Tools.Dispatch;

internal sealed class CalculateDistanceTool(ITenantUnitOfWork tenantUow)
    : AgentTool<CalculateDistanceTool.Input>, IAgentToolMetadata
{
    /// <summary>Roads run longer than the straight line between two points.</summary>
    private const double RoadFactor = 1.3;

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
        "Estimate the driving distance (in the tenant's unit, named in each key) and drive time between two geographic points.")
    {
        RequiredPermission = Permission.Dispatch.View,
        Surfaces = AgentSurfaces.All
    };

    protected override Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var origin = new GeoPoint(input.OriginLng, input.OriginLat);
        var destination = new GeoPoint(input.DestLng, input.DestLat);
        var straightLineMeters = origin.DistanceTo(destination);
        var drivingMeters = straightLineMeters * RoadFactor;

        var unit = tenantUow.GetCurrentTenant().Settings.DistanceUnit;
        var suffix = unit.Abbreviation();

        return Task.FromResult(ToolResult.Ok(new Dictionary<string, object?>
        {
            [$"straight_line_{suffix}"] = DispatchUnits.FromMeters(straightLineMeters, unit),
            [$"estimated_driving_{suffix}"] = DispatchUnits.FromMeters(drivingMeters, unit),
            ["drive_time"] = AgentDuration.Format(
                HosFeasibility.EstimateDrivingMinutes(DispatchUnits.MetersToKm(drivingMeters)))
        }));
    }
}
