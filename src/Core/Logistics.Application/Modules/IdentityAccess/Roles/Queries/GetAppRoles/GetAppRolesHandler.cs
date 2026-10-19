using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Roles.Queries;

internal sealed class GetAppRolesHandler(IMasterUnitOfWork masterUow)
    : IRequestHandler<GetAppRolesQuery, PagedResult<RoleDto>>
{
    public Task<PagedResult<RoleDto>> Handle(
        GetAppRolesQuery req, CancellationToken ct)
    {
        var query = masterUow.Repository<AppRole>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            var search = req.Search.ToLower();
            query = query.Where(i =>
                (i.Name != null && i.Name.ToLower().Contains(search)) ||
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
