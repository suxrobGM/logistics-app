using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class GetFleetOverviewTool(ITenantUnitOfWork tenantUow) : IDispatchTool
{
    public string Name => "get_fleet_overview";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        var totalTrucks = await tenantUow.Repository<Truck>().CountAsync(ct: ct);
        var availableTrucks = await tenantUow.Repository<Truck>()
            .CountAsync(t => t.Status == TruckStatus.Available, ct);
        var unassignedLoads = await tenantUow.Repository<Load>()
            .CountAsync(l => l.Status == LoadStatus.Draft && l.TripStops.Count == 0, ct);
        var activeTrips = await tenantUow.Repository<Trip>()
            .CountAsync(t => t.Status == TripStatus.Dispatched || t.Status == TripStatus.InTransit, ct);
        var hosViolations = await tenantUow.Repository<DriverHosStatus>()
            .CountAsync(h => h.IsInViolation, ct);

        return JsonSerializer.Serialize(new
        {
            total_trucks = totalTrucks,
            available_trucks = availableTrucks,
            unassigned_loads = unassignedLoads,
            active_trips = activeTrips,
            drivers_in_violation = hosViolations
        });
    }
}
