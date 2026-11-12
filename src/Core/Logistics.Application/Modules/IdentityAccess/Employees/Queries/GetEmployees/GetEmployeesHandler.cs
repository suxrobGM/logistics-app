using Logistics.Application.Abstractions;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Employees.Queries;

internal sealed class GetEmployeesHandler(ITenantUnitOfWork tenantUow) : IRequestHandler<GetEmployeesQuery, PagedResult<EmployeeDto>>
{
    public async Task<PagedResult<EmployeeDto>> Handle(
        GetEmployeesQuery req,
        CancellationToken ct)
    {
        var query = tenantUow.Repository<Employee>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            var search = req.Search.ToLower();
            query = query.Where(i =>
                (i.FirstName != null && i.FirstName.ToLower().Contains(search)) ||
                (i.LastName != null && i.LastName.ToLower().Contains(search)) ||
                (i.PhoneNumber != null && i.PhoneNumber.ToLower().Contains(search)) ||
                (i.Email != null && i.Email.ToLower().Contains(search)));
        }

        if (req.Roles.Length > 0)
        {
            query = query.Where(e => e.Role != null && req.Roles.Contains(e.Role.Name));
        }

        return await query.ToPagedResultAsync(req, i => i.ToDto(), ct);
    }
}
