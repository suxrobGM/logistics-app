namespace Logistics.Application.Handlers.Queries;

internal abstract class GetPagedQueryHandlerBase<TQuery, TResponse> 
    : RequestHandlerBase<GetPagedQueryBase<TResponse>, PagedDataResult<TResponse>>
    where TQuery : GetPagedQueryBase<TResponse>
{
    protected override bool Validate(GetPagedQueryBase<TResponse> request, out string errorDescription)
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
