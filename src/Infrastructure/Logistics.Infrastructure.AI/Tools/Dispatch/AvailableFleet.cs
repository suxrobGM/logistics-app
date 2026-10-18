using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Tools.Dispatch;

internal sealed record AvailableTruck(Truck Truck, Employee? Driver, DriverHosStatus? Hos, Trailer? Trailer);

/// <summary>
/// Available trucks with driver, HOS row and hooked trailer. Each is a lazy navigation, so they are
/// batch-read rather than queried per truck.
/// </summary>
internal static class AvailableFleet
{
    public static async Task<List<AvailableTruck>> LoadAsync(ITenantUnitOfWork tenantUow, CancellationToken ct)
    {
        var trucks = await tenantUow.Repository<Truck>()
            .GetListAsync(t => t.Status == TruckStatus.Available, ct);

        var driverIds = trucks
            .Where(t => t.MainDriverId is not null)
            .Select(t => t.MainDriverId!.Value)
            .ToList();

        var hosStatuses = driverIds.Count > 0
            ? (await tenantUow.Repository<DriverHosStatus>()
                .GetListAsync(h => driverIds.Contains(h.EmployeeId), ct))
                .ToDictionary(h => h.EmployeeId)
            : [];

        var drivers = driverIds.Count > 0
            ? (await tenantUow.Repository<Employee>()
                .GetListAsync(e => driverIds.Contains(e.Id), ct))
                .ToDictionary(e => e.Id)
            : [];

        var trailerIds = trucks
            .Where(t => t.CurrentTrailerId is not null)
            .Select(t => t.CurrentTrailerId!.Value)
            .ToList();

        var trailers = trailerIds.Count > 0
            ? (await tenantUow.Repository<Trailer>()
                .GetListAsync(t => trailerIds.Contains(t.Id), ct))
                .ToDictionary(t => t.Id)
            : [];

        return [.. trucks.Select(truck => new AvailableTruck(
            truck,
            truck.MainDriverId is { } driverId ? drivers.GetValueOrDefault(driverId) : null,
            truck.MainDriverId is { } hosId ? hosStatuses.GetValueOrDefault(hosId) : null,
            truck.CurrentTrailerId is { } trailerId ? trailers.GetValueOrDefault(trailerId) : null))];
    }
}
