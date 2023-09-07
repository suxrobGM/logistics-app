using Logistics.Application.Tenant.Mappers;
using Logistics.Domain.Enums;
using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetDriversHandler : RequestHandler<GetDriversQuery, PagedResponseResult<EmployeeDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetDriversHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }
    
    protected override async Task<PagedResponseResult<EmployeeDto>> HandleValidated(
        GetDriversQuery req, CancellationToken cancellationToken)
    {
        var totalItems = _tenantRepository.Query<Employee>().Count();
        var driverRole = await _tenantRepository.GetAsync<TenantRole>(i => i.Name == TenantRoles.Driver);

        if (driverRole == null)
            return PagedResponseResult<EmployeeDto>.CreateError("Could not found the driver role");
        
        var employeesDto = _tenantRepository.Query<Employee>()
            .Where(i => i.Roles.Contains(driverRole))
            .OrderBy(i => req.OrderBy)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(i => i.ToDto())
            .ToArray();

        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return new PagedResponseResult<EmployeeDto>(employeesDto, totalItems, totalPages);
    }
}
