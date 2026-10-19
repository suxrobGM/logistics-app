using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.Operations.Loads.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Logistics.Mediator;

namespace Logistics.Infrastructure.AI.Tools.Dispatch;

internal sealed class GetUnassignedLoadsTool(IMediator mediator)
    : AgentTool<NoToolInput>, IAgentToolMetadata
{
    public static AgentToolDefinition Definition => new(
        "get_unassigned_loads",
        "Get the Draft loads not assigned to any trip (up to 50; total says how many exist). Returns load ID, name, type, origin, destination, distance, delivery cost, customer, hazmat_class on hazmat loads, and for car hauler loads vehicle_count and vehicles (compare with a truck's vehicle_capacity).")
    {
        RequiredPermission = Permission.Dispatch.View,
        Surfaces = AgentSurfaces.All
    };

    protected override async Task<string> ExecuteAsync(NoToolInput input, CancellationToken ct)
    {
        var result = await mediator.Send(new GetUnassignedLoadsQuery { PageSize = ToolResult.MaxUnassignedLoads }, ct);

        if (!result.IsSuccess)
            return ToolResult.Error(result.Error ?? "Unknown error");

        var items = result.Value?.ToList() ?? [];
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var loads = items.Select(l => new AgentToolLoadDto
        {
            Id = l.Id,
            Name = l.Name,
            Type = l.Type.ToString(),
            Origin = l.OriginAddress?.ToString(),
            Destination = l.DestinationAddress?.ToString(),
            OriginLat = l.OriginLocation?.Latitude,
            OriginLng = l.OriginLocation?.Longitude,
            DestLat = l.DestinationLocation?.Latitude,
            DestLng = l.DestinationLocation?.Longitude,
            DistanceKm = Math.Round(DispatchUnits.MetersToKm(l.Distance), 1),
            DeliveryCost = l.DeliveryCost,
            Customer = l.Customer?.Name,
            ContainerNumber = l.ContainerNumber,
            ContainerIsoType = l.ContainerIsoType?.ToString(),
            OriginTerminal = FormatTerminal(l.OriginTerminalName, l.OriginTerminalCode),
            DestinationTerminal = FormatTerminal(l.DestinationTerminalName, l.DestinationTerminalCode),
            ContainerLastFreeDay = l.ContainerLastFreeDay,
            ContainerDaysUntilLastFreeDay = l.ContainerLastFreeDay is { } lfd ? lfd.DayNumber - today.DayNumber : null,
            ContainerOnHold = l.ContainerIsOnHold,
            HazmatClass = l.HazmatClass?.ToString(),
            VehicleCount = l.Vehicles.Count > 0 ? l.Vehicles.Count : null,
            Vehicles = l.Vehicles.Count > 0 ? AgentVehicleSummary.Describe(l.Vehicles) : null
        }).ToList();

        return ToolResult.Typed(new AgentToolResultDto { Loads = loads, Count = items.Count, Total = result.TotalItems });
    }

    /// <summary>"Los Angeles (USLAX)" - null when the load has no terminal, so it stays out of the JSON.</summary>
    private static string? FormatTerminal(string? name, string? code)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.IsNullOrWhiteSpace(code) ? null : code;

        return string.IsNullOrWhiteSpace(code) ? name : $"{name} ({code})";
    }
}
