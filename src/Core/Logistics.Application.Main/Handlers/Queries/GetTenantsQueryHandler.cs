namespace Logistics.Application.Handlers.Queries;

internal sealed class GetTenantsQueryHandler : RequestHandlerBase<GetTenantsQuery, PagedDataResult<TenantDto>>
{
    protected override Task<PagedDataResult<TenantDto>> HandleValidated(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
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
