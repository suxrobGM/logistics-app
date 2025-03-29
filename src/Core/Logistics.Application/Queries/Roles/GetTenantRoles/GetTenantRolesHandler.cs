using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Specifications;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetTenantRolesHandler : RequestHandler<GetTenantRolesQuery, PagedResult<RoleDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetTenantRolesHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<PagedResult<RoleDto>> HandleValidated(
        GetTenantRolesQuery req, CancellationToken cancellationToken)
    {
        var totalItems = await _tenantUow.Repository<TenantRole>().CountAsync();

        var rolesDto = _tenantUow.Repository<TenantRole>()
            .ApplySpecification(new SearchTenantRoles(req.Search, req.Page, req.PageSize))
            .Select(i => i.ToDto())
            .ToArray();
        
        return PagedResult<RoleDto>.Succeed(rolesDto, totalItems, req.PageSize);
    }
}
