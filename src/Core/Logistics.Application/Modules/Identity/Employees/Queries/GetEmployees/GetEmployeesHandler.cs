using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Identity.Employees.Queries;

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

        return await query.ToPagedResultAsync(req, MapPageAsync, ct);
    }

    private async Task<IEnumerable<EmployeeDto>> MapPageAsync(IReadOnlyList<Employee> employees, CancellationToken ct)
    {
        var roleIds = employees.Select(e => e.RoleId).OfType<Guid>().Distinct().ToArray();
        var roles = await tenantUow.Repository<TenantRole>().Query()
            .Where(r => roleIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, ct);

        return employees.Select(e => e.ToDto(e.RoleId is { } roleId ? roles.GetValueOrDefault(roleId) : null));
    }
}
