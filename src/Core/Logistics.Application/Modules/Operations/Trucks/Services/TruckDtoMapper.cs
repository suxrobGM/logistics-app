using Logistics.Application.Modules.IdentityAccess.Employees;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Trucks.Services;

internal sealed class TruckDtoMapper(ITenantUnitOfWork tenantUow) : ITruckDtoMapper
{
    public async Task<Dictionary<Guid, TruckDto>> MapAsync(IReadOnlyCollection<Truck> trucks, CancellationToken ct)
    {
        var drivers = await GetDriversAsync(trucks, ct);

        return trucks.ToDictionary(t => t.Id, t => t.ToDto(
            t.MainDriverId is { } mainId ? drivers.GetValueOrDefault(mainId) : null,
            t.SecondaryDriverId is { } secondaryId ? drivers.GetValueOrDefault(secondaryId) : null));
    }

    private async Task<Dictionary<Guid, EmployeeDto>> GetDriversAsync(IReadOnlyCollection<Truck> trucks, CancellationToken ct)
    {
        var driverIds = trucks.Select(t => t.MainDriverId)
            .Concat(trucks.Select(t => t.SecondaryDriverId))
            .OfType<Guid>()
            .Distinct()
            .ToArray();
        if (driverIds.Length == 0)
        {
            return [];
        }

        var employees = await tenantUow.Repository<Employee>().Query()
            .Where(e => driverIds.Contains(e.Id))
            .ToListAsync(ct);

        var roleIds = employees.Select(e => e.RoleId).OfType<Guid>().Distinct().ToArray();
        var roles = roleIds.Length == 0
            ? []
            : await tenantUow.Repository<TenantRole>().Query()
                .Where(r => roleIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, ct);

        return employees.ToDictionary(
            e => e.Id,
            e => e.ToDto(e.RoleId is { } roleId ? roles.GetValueOrDefault(roleId) : null));
    }
}
