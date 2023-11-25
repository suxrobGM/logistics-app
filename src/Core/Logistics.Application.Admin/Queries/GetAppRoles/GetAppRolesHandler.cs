using Logistics.Application.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Specifications;
using Logistics.Shared.Models;
using Logistics.Shared;

namespace Logistics.Application.Admin.Queries;

internal sealed class GetAppRolesHandler : RequestHandler<GetAppRolesQuery, PagedResponseResult<AppRoleDto>>
{
    private readonly IMasterUnityOfWork _masterUow;

    public GetAppRolesHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<PagedResponseResult<AppRoleDto>> HandleValidated(
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
        
        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return PagedResponseResult<AppRoleDto>.Create(rolesDto, totalItems, totalPages);
    }
}
