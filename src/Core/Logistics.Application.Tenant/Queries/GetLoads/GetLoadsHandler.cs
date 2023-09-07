using Logistics.Application.Tenant.Mappers;
using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetLoadsHandler : RequestHandler<GetLoadsQuery, PagedResponseResult<LoadDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetLoadsHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override Task<PagedResponseResult<LoadDto>> HandleValidated(
        GetLoadsQuery req, 
        CancellationToken cancellationToken)
    {
        var totalItems = _tenantRepository.Query<Load>().Count();
        var spec = new SearchLoads(req.Search, req.OrderBy, req.Descending);

        var loads = _tenantRepository.ApplySpecification(spec)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToArray();

        var loadsDto = loads.Select(i => i.ToDto());
        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return Task.FromResult(new PagedResponseResult<LoadDto>(loadsDto, totalItems, totalPages));
    }
}
