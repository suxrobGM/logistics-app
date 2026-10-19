using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trucks.Services;

internal sealed class TruckDriverFinder(ITenantUnitOfWork tenantUow) : ITruckDriverFinder
{
    public async Task<Result<Employee>> FindAssignableDriverAsync(Guid driverId, Guid? truckId, CancellationToken ct)
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
