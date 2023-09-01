using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetTenantRolesHandler : RequestHandler<GetTenantRolesQuery, PagedResponseResult<TenantRoleDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetTenantRolesHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override Task<PagedResponseResult<TenantRoleDto>> HandleValidated(
        GetTenantRolesQuery query, CancellationToken cancellationToken)
    {
        var totalItems = _tenantRepository.Query<TenantRole>().Count();

        var rolesDto = _tenantRepository
            .ApplySpecification(new SearchTenantRoles(query.Search))
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(i => new TenantRoleDto()
            {
                Name = i.Name,
                DisplayName = i.DisplayName
            })
            .ToArray();
        
        var totalPages = (int)Math.Ceiling(totalItems / (double)query.PageSize);
        return Task.FromResult(new PagedResponseResult<TenantRoleDto>(rolesDto, totalItems, totalPages));
    }

    protected override bool Validate(GetTenantRolesQuery query, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (query.Page <= 0)
        {
            errorDescription = "Page number should be non-negative";
        }
        else if (query.PageSize <= 1)
        {
            errorDescription = "Page size should be greater than one";
        }
        return true;
    }
}