using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetEmployeesHandler : RequestHandler<GetEmployeesQuery, PagedResult<EmployeeDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetEmployeesHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<PagedResult<EmployeeDto>> HandleValidated(
        GetEmployeesQuery req, 
        CancellationToken cancellationToken)
    {
        var totalItems = await _tenantUow.Repository<Employee>().CountAsync();
        var specification = new SearchEmployees(req.Search, req.Role, req.OrderBy, req.Page, req.PageSize);

        var employeeDto = _tenantUow.Repository<Employee>().ApplySpecification(specification)
            .Select(employeeEntity => employeeEntity.ToDto())
            .ToArray();
        
        return PagedResult<EmployeeDto>.Succeed(employeeDto, totalItems, req.PageSize);
    }
}
