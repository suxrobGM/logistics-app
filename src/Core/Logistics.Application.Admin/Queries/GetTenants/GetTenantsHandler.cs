using Logistics.Models;

namespace Logistics.Application.Admin.Queries;

internal sealed class GetTenantsHandler : RequestHandler<GetTenantsQuery, PagedResponseResult<TenantDto>>
{
    private readonly IMainRepository _repository;

    public GetTenantsHandler(IMainRepository repository)
    {
        _repository = repository;
    }

    protected override Task<PagedResponseResult<TenantDto>> HandleValidated(GetTenantsQuery req, CancellationToken cancellationToken)
    {
        var totalItems = _repository.Query<Tenant>().Count();
        var spec = new SearchTenants(req.Search, req.OrderBy, req.Descending);

        var items = _repository
            .ApplySpecification(spec)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(i => new TenantDto
            {
                Id = i.Id,
                Name = i.Name,
                DisplayName = i.DisplayName,
                ConnectionString = req.IncludeConnectionStrings ? i.ConnectionString : null,
            })
            .ToArray();

        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return Task.FromResult(new PagedResponseResult<TenantDto>(items, totalItems, totalPages));
    }
}
