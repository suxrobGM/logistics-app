using Logistics.Shared.Enums;

namespace Logistics.Application.Tenant.Commands;

internal sealed class UpdateEmployeeHandler : RequestHandler<UpdateEmployeeCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;

    public UpdateEmployeeHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        UpdateEmployeeCommand req, CancellationToken cancellationToken)
    {
        var employeeEntity = await _tenantRepository.GetAsync<Employee>(req.UserId);
        var tenantRole = await _tenantRepository.GetAsync<TenantRole>(i => i.Name == req.Role);

        if (employeeEntity == null)
            return ResponseResult.CreateError("Could not find the specified user");

        if (tenantRole != null)
        {
            employeeEntity.Roles.Clear();
            employeeEntity.Roles.Add(tenantRole);
        }
        if (req.SalaryType.HasValue && employeeEntity.SalaryType != req.SalaryType)
        {
            employeeEntity.SalaryType = req.SalaryType.Value;
        }
        if (req.Salary.HasValue && employeeEntity.Salary != req.Salary)
        {
            employeeEntity.Salary = req.SalaryType == SalaryType.None ? 0 : req.Salary.Value;
        }
        
        _tenantRepository.Update(employeeEntity);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
