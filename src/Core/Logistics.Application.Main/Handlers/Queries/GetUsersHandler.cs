namespace Logistics.Application.Main.Handlers.Queries;

internal sealed class GetUsersHandler : RequestHandlerBase<GetUsersRequest, PagedResponseResult<UserDto>>
{
    private readonly IMainRepository _repository;

    public GetUsersHandler(
        IMainRepository repository)
    {
        _repository = repository;
    }

    protected override Task<PagedResponseResult<UserDto>> HandleValidated(
        GetUsersRequest request, 
        CancellationToken cancellationToken)
    {
        var totalItems = _repository.Query<User>().Count();

        var users = _repository
            .ApplySpecification(new SearchUsers(request.Search))
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new UserDto
            {
                Id = i.Id,
                UserName = i.UserName!,
                FirstName = i.FirstName,
                LastName = i.LastName,
                Email = i.Email,
                PhoneNumber = i.PhoneNumber
            })
            .ToArray();

        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);
        return Task.FromResult(new PagedResponseResult<UserDto>(users, totalItems, totalPages));
    }

    protected override bool Validate(GetUsersRequest request, out string errorDescription)
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
