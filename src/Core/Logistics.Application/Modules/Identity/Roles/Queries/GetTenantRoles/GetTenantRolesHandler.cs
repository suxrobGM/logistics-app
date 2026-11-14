using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Identity.Roles.Queries;

internal sealed class GetTenantRolesHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetTenantRolesQuery, PagedResult<RoleDto>>
{
    private static readonly SortFields<TenantRole> Sorts = new SortFields<TenantRole>("name")
        .Add("name", x => x.Name);

    public async Task<PagedResult<RoleDto>> Handle(
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

        return await query.ToPagedResultAsync(req, Sorts, i => i.ToDto(), ct);
    }
}
