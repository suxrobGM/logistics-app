using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetAppRolesHandler : RequestHandler<GetAppRolesQuery, PagedResult<RoleDto>>
{
    private readonly IMasterUnityOfWork _masterUow;

    public GetAppRolesHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<PagedResult<RoleDto>> HandleValidated(
        GetAppRolesQuery req, CancellationToken cancellationToken)
    {
        var totalItems = await _masterUow.Repository<AppRole>().CountAsync();

        var rolesDto = _masterUow.Repository<AppRole>()
            .ApplySpecification(new SearchAppRoles(req.Search, req.Page, req.PageSize))
            .Select(i => i.ToDto())
            .ToArray();
        
        return PagedResult<RoleDto>.Succeed(rolesDto, totalItems, req.PageSize);
    }
}
