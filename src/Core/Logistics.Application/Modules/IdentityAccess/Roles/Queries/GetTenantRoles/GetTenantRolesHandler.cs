using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Roles.Queries;

internal sealed class GetTenantRolesHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetTenantRolesQuery, PagedResult<RoleDto>>
{
    public Task<PagedResult<RoleDto>> Handle(
        GetTenantRolesQuery req, CancellationToken ct)
    {
        var query = tenantUow.Repository<TenantRole>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            var search = req.Search.ToLower();
            query = query.Where(i =>
                i.Name.ToLower().Contains(search) ||
                (i.DisplayName != null && i.DisplayName.ToLower().Contains(search)));
        }

        var totalItems = query.Count();

        var rolesDto = query
            .OrderBy(req.OrderBy, i => i.Id, defaultOrderBy: "Name")
            .ApplyPaging(req.Page, req.PageSize)
            .Select(i => i.ToDto())
            .ToArray();

        return Task.FromResult(PagedResult<RoleDto>.Ok(rolesDto, totalItems, req.PageSize));
    }
}
