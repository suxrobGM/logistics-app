using Logistics.Application.Tenant.Mappers;
using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetEmployeesHandler : RequestHandler<GetEmployeesQuery, PagedResponseResult<EmployeeDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetEmployeesHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override Task<PagedResponseResult<EmployeeDto>> HandleValidated(
        GetEmployeesQuery req, 
        CancellationToken cancellationToken)
    {
        var totalItems = _tenantRepository.Query<Employee>().Count();
        
        var employees = _tenantRepository.Query<Employee>()
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .OrderBy(req.OrderBy)
            .ToArray();

        var employeesDto = employees.Select(employeeEntity => employeeEntity.ToDto());
        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return Task.FromResult(new PagedResponseResult<EmployeeDto>(employeesDto, totalItems, totalPages));
    }
}
