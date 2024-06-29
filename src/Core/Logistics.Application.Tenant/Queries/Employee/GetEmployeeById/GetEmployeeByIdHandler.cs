using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetEmployeeByIdHandler : RequestHandler<GetEmployeeByIdQuery, Result<EmployeeDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetEmployeeByIdHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result<EmployeeDto>> HandleValidated(
        GetEmployeeByIdQuery req, CancellationToken cancellationToken)
    {
        var employeeEntity = await _tenantUow.Repository<Employee>().GetByIdAsync(req.UserId);

        if (employeeEntity is null)
        {
            return Result<EmployeeDto>.Fail($"Could not find the specified employee with ID {req.UserId}");
        }

        var employeeDto = employeeEntity.ToDto();
        return Result<EmployeeDto>.Succeed(employeeDto);
    }
}
