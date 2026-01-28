using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateEmployeeHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<UpdateEmployeeCommand, Result>
{
    public async Task<Result> Handle(
        UpdateEmployeeCommand req, CancellationToken ct)
    {
        var employeeEntity = await tenantUow.Repository<Employee>().GetByIdAsync(req.UserId, ct);
        var tenantRole = await tenantUow.Repository<TenantRole>().GetAsync(i => i.Name == req.Role, ct);

        if (employeeEntity is null)
        {
            return Result.Fail("Could not find the specified user");
        }

        if (tenantRole is not null)
        {
            employeeEntity.Role = tenantRole;
        }

        if (req.SalaryType.HasValue && employeeEntity.SalaryType != req.SalaryType)
        {
            employeeEntity.SalaryType = req.SalaryType.Value;
        }

        if (req.Salary.HasValue && employeeEntity.Salary != req.Salary)
        {
            employeeEntity.Salary = req.SalaryType == SalaryType.None ? 0 : req.Salary.Value;
        }

        if (req.Status.HasValue && employeeEntity.Status != req.Status)
        {
            employeeEntity.Status = req.Status.Value;
        }

        tenantUow.Repository<Employee>().Update(employeeEntity);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
