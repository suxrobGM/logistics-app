using Logistics.Application.Abstractions;
using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetTenantsHandler : RequestHandler<GetTenantsQuery, PagedResult<TenantDto>>
{
    private readonly IMasterUnitOfWork _masterUow;

    public GetTenantsHandler(IMasterUnitOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    public override async Task<PagedResult<TenantDto>> Handle(
        GetTenantsQuery req, CancellationToken ct)
    {
        var totalItems = await _masterUow.Repository<Tenant>().CountAsync();
        var spec = new SearchTenants(req.Search, req.OrderBy, req.Page, req.PageSize);

        var items = _masterUow.Repository<Tenant>()
            .ApplySpecification(spec)
            .Select(i => i.ToDto(req.IncludeConnectionStrings, null))
            .ToArray();

        return PagedResult<TenantDto>.Succeed(items, totalItems, req.PageSize);
    }
}
