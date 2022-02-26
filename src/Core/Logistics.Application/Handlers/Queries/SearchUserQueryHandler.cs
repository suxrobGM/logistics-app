namespace Logistics.Application.Handlers.Queries;

internal sealed class SearchUserQueryHandler : RequestHandlerBase<SearchUserQuery, PagedDataResult<UserDto>>
{
    protected override Task<PagedDataResult<UserDto>> HandleValidated(SearchUserQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override bool Validate(SearchUserQuery request, out string errorDescription)
    {
        throw new NotImplementedException();
    }
}
