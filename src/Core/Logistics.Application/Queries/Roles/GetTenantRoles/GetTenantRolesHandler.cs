using Logistics.Application.Abstractions;
using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetTenantRolesHandler : IAppRequestHandler<GetTenantRolesQuery, PagedResult<RoleDto>>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public GetTenantRolesHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public async Task<PagedResult<RoleDto>> Handle(
        GetTenantRolesQuery req, CancellationToken ct)
    {
        var totalItems = await _tenantUow.Repository<TenantRole>().CountAsync();

        var rolesDto = _tenantUow.Repository<TenantRole>()
            .ApplySpecification(new SearchTenantRoles(req.Search, req.Page, req.PageSize))
            .Select(i => i.ToDto())
            .ToArray();

        return PagedResult<RoleDto>.Succeed(rolesDto, totalItems, req.PageSize);
    }
}
