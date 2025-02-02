using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Specifications;
using Logistics.Shared;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetTenantRolesHandler : RequestHandler<GetTenantRolesQuery, PagedResult<TenantRoleDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetTenantRolesHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<PagedResult<TenantRoleDto>> HandleValidated(
        GetTenantRolesQuery req, CancellationToken cancellationToken)
    {
        var totalItems = await _tenantUow.Repository<TenantRole>().CountAsync();

        var rolesDto = _tenantUow.Repository<TenantRole>()
            .ApplySpecification(new SearchTenantRoles(req.Search, req.Page, req.PageSize))
            .Select(i => new TenantRoleDto()
            {
                Name = i.Name,
                DisplayName = i.DisplayName
            })
            .ToArray();
        
        return PagedResult<TenantRoleDto>.Succeed(rolesDto, totalItems, req.PageSize);
    }
}
