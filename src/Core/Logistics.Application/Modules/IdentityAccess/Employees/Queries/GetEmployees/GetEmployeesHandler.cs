using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Employees.Queries;

internal sealed class GetEmployeesHandler(ITenantUnitOfWork tenantUow) : IAppRequestHandler<GetEmployeesQuery, PagedResult<EmployeeDto>>
{
    public Task<PagedResult<EmployeeDto>> Handle(
        GetEmployeesQuery req,
        CancellationToken ct)
    {
        var query = tenantUow.Repository<Employee>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            query = query.Where(i =>
                (i.FirstName != null && i.FirstName.Contains(req.Search)) ||
                (i.LastName != null && i.LastName.Contains(req.Search)) ||
                (i.PhoneNumber != null && i.PhoneNumber.Contains(req.Search)) ||
                (i.Email != null && i.Email.Contains(req.Search)));
        }

        if (!string.IsNullOrEmpty(req.Role))
        {
            query = query.Where(e => e.Role != null && e.Role.Name.Contains(req.Role));
        }

        var totalItems = query.Count();

        var employeeDto = query
            .OrderBy(req.OrderBy)
            .ApplyPaging(req.Page, req.PageSize)
            .Select(employeeEntity => employeeEntity.ToDto())
            .ToArray();

        return Task.FromResult(PagedResult<EmployeeDto>.Ok(employeeDto, totalItems, req.PageSize));
    }
}
