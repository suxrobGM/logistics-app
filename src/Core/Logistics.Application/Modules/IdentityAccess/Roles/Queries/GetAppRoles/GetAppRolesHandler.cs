using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Roles.Queries;

internal sealed class GetAppRolesHandler(IMasterUnitOfWork masterUow)
    : IAppRequestHandler<GetAppRolesQuery, PagedResult<RoleDto>>
{
    public Task<PagedResult<RoleDto>> Handle(
        GetAppRolesQuery req, CancellationToken ct)
    {
        var query = masterUow.Repository<AppRole>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            query = query.Where(i =>
                (i.Name != null && i.Name.Contains(req.Search)) ||
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
