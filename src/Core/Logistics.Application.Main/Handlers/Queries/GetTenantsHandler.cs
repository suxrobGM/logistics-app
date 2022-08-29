namespace Logistics.Application.Handlers.Queries;

internal sealed class GetTenantsHandler : RequestHandlerBase<GetTenantsQuery, PagedDataResult<TenantDto>>
{
    private readonly IMainRepository _repository;

    public GetTenantsHandler(IMainRepository repository)
    {
        _repository = repository;
    }

    protected override Task<PagedDataResult<TenantDto>> HandleValidated(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        var totalItems = _repository.GetQuery<Tenant>().Count();
        var itemsQuery = _repository.GetQuery<Tenant>();

        if (!string.IsNullOrEmpty(request.Search))
        {
            itemsQuery = _repository.GetQuery<Tenant>(i => i.Name!.Contains(request.Search) || i.DisplayName!.Contains(request.Search));
        }

        var items = itemsQuery
            .OrderBy(i => i.Id)
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
