using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Employees.Commands;

internal sealed class DeleteEmployeeHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow) : IAppRequestHandler<DeleteEmployeeCommand, Result>
{
    public async Task<Result> Handle(
        DeleteEmployeeCommand req, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();
        var employee = await tenantUow.Repository<Employee>().GetByIdAsync(req.UserId);

        if (employee is null)
        {
            return Result.Fail($"Could not find employee with ID {req.UserId}");
        }

        var user = await masterUow.Repository<User>().GetByIdAsync(employee.Id);

        // Remove tenant from user if it matches the current tenant
        if (user?.Tenant != null && user.Tenant.Id == tenant.Id)
        {
            user.Tenant = null;
        }

        var employeeLoads = tenantUow.Repository<Load>()
            .Query()
            .Where(i => i.AssignedDispatcherId == employee.Id);

        foreach (var load in employeeLoads)
        {
            if (load.AssignedDispatcherId == employee.Id)
            {
                load.AssignedDispatcher = null;
            }
        }

        tenantUow.Repository<Employee>().Delete(employee);
        await tenantUow.SaveChangesAsync();
        await masterUow.SaveChangesAsync();
        return Result.Ok();
    }
}
