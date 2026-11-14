using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Identity.Roles.Queries;

internal sealed class GetAppRolesHandler(IMasterUnitOfWork masterUow)
    : IRequestHandler<GetAppRolesQuery, PagedResult<RoleDto>>
{
    private static readonly SortFields<AppRole> Sorts = new SortFields<AppRole>("name")
        .Add("name", x => x.Name);

    public async Task<PagedResult<RoleDto>> Handle(
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

        return await query.ToPagedResultAsync(req, Sorts, i => i.ToDto(), ct);
    }
}
