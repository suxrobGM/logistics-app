using Logistics.Application.Tenant.Mappers;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetEmployeesHandler : RequestHandler<GetEmployeesQuery, PagedResponseResult<EmployeeDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetEmployeesHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<PagedResponseResult<EmployeeDto>> HandleValidated(
        GetEmployeesQuery req, 
        CancellationToken cancellationToken)
    {
        var totalItems = _tenantRepository.Query<Employee>().Count();
        var employeesQuery = _tenantRepository.Query<Employee>();
        var specification = new SearchEmployees(req.Search, req.OrderBy, req.Descending);

        if (!string.IsNullOrEmpty(req.Role))
        {
            var role = await _tenantRepository.GetAsync<TenantRole>(i => i.Name.Contains(req.Role));
            if (role is not null)
            {
                employeesQuery = _tenantRepository
                    .Query<EmployeeTenantRole>()
                    .Where(i => i.RoleId == role.Id)
                    .Select(i => i.Employee);
            }
        }
        
        employeesQuery = employeesQuery.ApplySpecification(specification)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize);

        var employeeDto = employeesQuery.Select(employeeEntity => employeeEntity.ToDto()).ToArray();
        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return new PagedResponseResult<EmployeeDto>(employeeDto, totalItems, totalPages);
    }
}
