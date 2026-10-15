using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Common.Services;

internal sealed class VehicleCapacityChecker(ITenantUnitOfWork tenantUow) : IVehicleCapacityChecker
{
    public async Task<string?> GetWarningAsync(Truck truck, Load? candidate = null, CancellationToken ct = default)
    {
        if (truck.VehicleCapacity <= 0)
        {
            return null;
        }

        var openLoads = await tenantUow.Repository<Load>().Query()
            .Where(l => l.AssignedTruckId == truck.Id
                        && l.Type == LoadType.Vehicle
                        && l.Status != LoadStatus.Delivered
                        && l.Status != LoadStatus.Cancelled)
            .ToListAsync(ct);

        if (candidate is not null && openLoads.All(l => l.Id != candidate.Id))
        {
            openLoads.Add(candidate);
        }

        return GetWarning(truck.Number, truck.VehicleCapacity, openLoads.Sum(l => l.Vehicles.Count));
    }

    internal static string? GetWarning(string truckNumber, int capacity, int vehicleCount) =>
        capacity > 0 && vehicleCount > capacity
            ? $"Truck {truckNumber} carries up to {capacity} vehicles, but its open loads have {vehicleCount}."
            : null;
}
