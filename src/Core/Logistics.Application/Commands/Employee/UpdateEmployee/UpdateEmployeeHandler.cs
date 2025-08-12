using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateEmployeeHandler : RequestHandler<UpdateEmployeeCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public UpdateEmployeeHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result> HandleValidated(
        UpdateEmployeeCommand req, CancellationToken ct)
    {
        var employeeEntity = await _tenantUow.Repository<Employee>().GetByIdAsync(req.UserId);
        var tenantRole = await _tenantUow.Repository<TenantRole>().GetAsync(i => i.Name == req.Role);

        if (employeeEntity is null) return Result.Fail("Could not find the specified user");

        if (tenantRole is not null)
        {
            employeeEntity.Roles.Clear();
            employeeEntity.Roles.Add(tenantRole);
        }

        if (req.SalaryType.HasValue && employeeEntity.SalaryType != req.SalaryType)
            employeeEntity.SalaryType = req.SalaryType.Value;
        if (req.Salary.HasValue && employeeEntity.Salary != req.Salary)
            employeeEntity.Salary = req.SalaryType == SalaryType.None ? 0 : req.Salary.Value;

        _tenantUow.Repository<Employee>().Update(employeeEntity);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
