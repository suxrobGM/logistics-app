using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class RemoveEmployeeRoleHandler : IAppRequestHandler<RemoveRoleFromEmployeeCommand, Result>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public RemoveEmployeeRoleHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public async Task<Result> Handle(
        RemoveRoleFromEmployeeCommand req, CancellationToken ct)
    {
        req.Role = req.Role.ToLower();
        var employee = await _tenantUow.Repository<Employee>().GetByIdAsync(req.UserId);

        if (employee is null)
        {
            return Result.Fail("Could not find the specified user");
        }

        var tenantRole = await _tenantUow.Repository<TenantRole>().GetAsync(i => i.Name == req.Role);

        if (tenantRole is null)
        {
            return Result.Fail("Could not find the specified role name");
        }

        employee.Roles.Remove(tenantRole);
        await _tenantUow.SaveChangesAsync();
        return Result.Ok();
    }
}
