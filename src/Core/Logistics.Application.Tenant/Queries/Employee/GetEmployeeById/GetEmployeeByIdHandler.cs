using Logistics.Application.Tenant.Mappers;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetEmployeeByIdHandler : RequestHandler<GetEmployeeByIdQuery, ResponseResult<EmployeeDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetEmployeeByIdHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult<EmployeeDto>> HandleValidated(
        GetEmployeeByIdQuery req, CancellationToken cancellationToken)
    {
        var employeeEntity = await _tenantRepository.GetAsync<Employee>(req.UserId);
        
        if (employeeEntity == null)
            return ResponseResult<EmployeeDto>.CreateError($"Could not find the specified employee with ID {req.UserId}");

        var employeeDto = employeeEntity.ToDto();
        return ResponseResult<EmployeeDto>.CreateSuccess(employeeDto);
    }
}
