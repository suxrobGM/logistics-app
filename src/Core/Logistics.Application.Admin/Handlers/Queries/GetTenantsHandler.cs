namespace Logistics.Application.Admin.Handlers.Queries;

internal sealed class GetTenantsHandler : RequestHandlerBase<GetTenantsRequest, PagedResponseResult<TenantDto>>
{
    private readonly IMainRepository _repository;

    public GetTenantsHandler(IMainRepository repository)
    {
        _repository = repository;
    }

    protected override Task<PagedResponseResult<TenantDto>> HandleValidated(GetTenantsRequest request, CancellationToken cancellationToken)
    {
        var totalItems = _repository.Query<Tenant>().Count();
        var spec = new SearchTenants(request.Search, request.OrderBy, request.Descending);

        var items = _repository
            .ApplySpecification(spec)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new TenantDto
            {
                Id = i.Id,
                Name = i.Name,
                DisplayName = i.DisplayName,
                ConnectionString = request.IncludeConnectionStrings ? i.ConnectionString : null,
            })
            .ToArray();

        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);
        return Task.FromResult(new PagedResponseResult<TenantDto>(items, totalItems, totalPages));
    }

    protected override bool Validate(GetTenantsRequest request, out string errorDescription)
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
