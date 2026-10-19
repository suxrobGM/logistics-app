using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trucks.Services;

internal static class TruckDriverAssignment
{
    /// <summary>
    ///     Loads the driver for a truck's main or secondary seat, refusing one who is not Active or who already
    ///     holds either seat on another truck. <paramref name="truckId" /> is the truck being edited, if any.
    /// </summary>
    public static async Task<Result<Employee>> FindAssignableDriverAsync(
        ITenantUnitOfWork tenantUow, Guid driverId, Guid? truckId, CancellationToken ct)
    {
        var driver = await tenantUow.Repository<Employee>().GetByIdAsync(driverId, ct);
        if (driver is null)
        {
            return Result<Employee>.Fail($"Could not find a driver with ID {driverId}");
        }

        if (driver.Status != EmployeeStatus.Active)
        {
            return Result<Employee>.Fail(
                $"Driver '{driver.GetFullName()}' is {driver.Status.GetDescription().ToLowerInvariant()} and cannot be assigned to a truck");
        }

        var otherTruck = await tenantUow.Repository<Truck>().GetAsync(
            t => t.Id != truckId && (t.MainDriverId == driverId || t.SecondaryDriverId == driverId), ct);
        if (otherTruck is not null)
        {
            return Result<Employee>.Fail(
                $"Driver '{driver.GetFullName()}' already drives truck '{otherTruck.Number}'");
        }

        return Result<Employee>.Ok(driver);
    }
}
