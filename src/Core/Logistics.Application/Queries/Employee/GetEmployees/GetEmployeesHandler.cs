using Logistics.Application.Abstractions;
using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetEmployeesHandler(ITenantUnitOfWork tenantUow) : IAppRequestHandler<GetEmployeesQuery, PagedResult<EmployeeDto>>
{
    public async Task<PagedResult<EmployeeDto>> Handle(
        GetEmployeesQuery req,
        CancellationToken ct)
    {
        var totalItems = await tenantUow.Repository<Employee>().CountAsync(ct: ct);
        var specification = new SearchEmployees(req.Search, req.Role, req.OrderBy, req.Page, req.PageSize);

        var employeeDto = tenantUow.Repository<Employee>().ApplySpecification(specification)
            .Select(employeeEntity => employeeEntity.ToDto())
            .ToArray();

        return PagedResult<EmployeeDto>.Succeed(employeeDto, totalItems, req.PageSize);
    }
}
