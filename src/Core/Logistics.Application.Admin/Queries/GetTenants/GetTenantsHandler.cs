using Logistics.Application.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Specifications;
using Logistics.Shared.Models;
using Logistics.Shared;

namespace Logistics.Application.Admin.Queries;

internal sealed class GetTenantsHandler : RequestHandler<GetTenantsQuery, PagedResponseResult<TenantDto>>
{
    private readonly IMasterUnityOfWork _masterUow;

    public GetTenantsHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<PagedResponseResult<TenantDto>> HandleValidated(GetTenantsQuery req, CancellationToken cancellationToken)
    {
        var totalItems = await _masterUow.Repository<Tenant>().CountAsync();
        var spec = new SearchTenants(req.Search, req.OrderBy, req.Page, req.PageSize, req.Descending);

        var items = _masterUow.Repository<Tenant>()
            .ApplySpecification(spec)
            .Select(i => i.ToDto(req.IncludeConnectionStrings))
            .ToArray();

        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return PagedResponseResult<TenantDto>.Create(items, totalItems, totalPages);
    }
}
