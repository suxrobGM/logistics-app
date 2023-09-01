using Logistics.Models;

namespace Logistics.Application.Admin.Queries;

internal sealed class GetTenantsHandler : RequestHandler<GetTenantsQuery, PagedResponseResult<TenantDto>>
{
    private readonly IMainRepository _repository;

    public GetTenantsHandler(IMainRepository repository)
    {
        _repository = repository;
    }

    protected override Task<PagedResponseResult<TenantDto>> HandleValidated(GetTenantsQuery query, CancellationToken cancellationToken)
    {
        var totalItems = _repository.Query<Tenant>().Count();
        var spec = new SearchTenants(query.Search, query.OrderBy, query.Descending);

        var items = _repository
            .ApplySpecification(spec)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(i => new TenantDto
            {
                Id = i.Id,
                Name = i.Name,
                DisplayName = i.DisplayName,
                ConnectionString = query.IncludeConnectionStrings ? i.ConnectionString : null,
            })
            .ToArray();

        var totalPages = (int)Math.Ceiling(totalItems / (double)query.PageSize);
        return Task.FromResult(new PagedResponseResult<TenantDto>(items, totalItems, totalPages));
    }
}
