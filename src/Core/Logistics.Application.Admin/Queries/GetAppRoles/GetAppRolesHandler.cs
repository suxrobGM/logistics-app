using Logistics.Application.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Specifications;
using Logistics.Shared.Models;
using Logistics.Shared;

namespace Logistics.Application.Admin.Queries;

internal sealed class GetAppRolesHandler : RequestHandler<GetAppRolesQuery, PagedResult<AppRoleDto>>
{
    private readonly IMasterUnityOfWork _masterUow;

    public GetAppRolesHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<PagedResult<AppRoleDto>> HandleValidated(
        GetAppRolesQuery req, CancellationToken cancellationToken)
    {
        var totalItems = await _masterUow.Repository<AppRole>().CountAsync();

        var rolesDto = _masterUow.Repository<AppRole>()
            .ApplySpecification(new SearchAppRoles(req.Search, req.Page, req.PageSize))
            .Select(i => new AppRoleDto()
            {
                Name = i.Name,
                DisplayName = i.DisplayName
            })
            .ToArray();
        
        return PagedResult<AppRoleDto>.Succeed(rolesDto, totalItems, req.PageSize);
    }
}
