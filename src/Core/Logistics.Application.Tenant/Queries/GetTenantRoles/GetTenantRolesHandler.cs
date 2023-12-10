using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetTenantRolesHandler : RequestHandler<GetTenantRolesQuery, PagedResponseResult<TenantRoleDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetTenantRolesHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<PagedResponseResult<TenantRoleDto>> HandleValidated(
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
        
        return PagedResponseResult<TenantRoleDto>.Create(rolesDto, totalItems, req.PageSize);
    }
}
