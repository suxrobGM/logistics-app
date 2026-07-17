using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Roles.Queries;

internal sealed class GetTenantRolesHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetTenantRolesQuery, PagedResult<RoleDto>>
{
    public Task<PagedResult<RoleDto>> Handle(
        GetTenantRolesQuery req, CancellationToken ct)
    {
        var query = tenantUow.Repository<TenantRole>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            query = query.Where(i =>
                i.Name.Contains(req.Search) ||
                (i.DisplayName != null && i.DisplayName.Contains(req.Search)));
        }

        var totalItems = query.Count();

        var rolesDto = query
            .ApplyPaging(req.Page, req.PageSize)
            .Select(i => i.ToDto())
            .ToArray();

        return Task.FromResult(PagedResult<RoleDto>.Ok(rolesDto, totalItems, req.PageSize));
    }
}
