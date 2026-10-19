using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trucks.Services;

internal static class TruckDriverAssignment
{
    /// <summary>
    ///     Loads the driver for a truck's main or secondary seat, refusing one who is not Active.
    /// </summary>
    public static async Task<Result<Employee>> FindAssignableDriverAsync(
        ITenantUnitOfWork tenantUow, Guid driverId, CancellationToken ct)
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

        return Result<Employee>.Ok(driver);
    }
}
