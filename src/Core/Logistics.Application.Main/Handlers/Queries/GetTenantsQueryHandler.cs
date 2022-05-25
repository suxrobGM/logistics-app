namespace Logistics.Application.Handlers.Queries;

internal sealed class GetTenantsQueryHandler : RequestHandlerBase<GetTenantsQuery, PagedDataResult<TenantDto>>
{
    private readonly IMainRepository<Tenant> _repository;

    public GetTenantsQueryHandler(IMainRepository<Tenant> repository)
    {
        _repository = repository;
    }

    protected override Task<PagedDataResult<TenantDto>> HandleValidated(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        var totalItems = _repository.GetQuery().Count();
        var itemsQuery = _repository.GetQuery();

        if (!string.IsNullOrEmpty(request.Search))
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            itemsQuery = _repository.GetQuery(i => i.Name.Contains(request.Search) || i.DisplayName.Contains(request.Search));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        var items = itemsQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new TenantDto
            {
                Id = i.Id,
                Name = i.Name,
                DisplayName = i.DisplayName,
                ConnectionString = i.ConnectionString
            })
            .ToArray();

        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);
        return Task.FromResult(new PagedDataResult<TenantDto>(items, totalItems, totalPages));
    }

    protected override bool Validate(GetTenantsQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (request.Page <= 0)
        {
            errorDescription = "Page number should be non-negative";
        }
        else if (request.PageSize <= 1)
        {
            errorDescription = "Page size should be greater than one";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
