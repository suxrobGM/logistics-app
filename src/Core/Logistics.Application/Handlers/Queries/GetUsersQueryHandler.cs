namespace Logistics.Application.Handlers.Queries;

internal sealed class GetUsersQueryHandler : GetPagedQueryHandlerBase<GetUsersQuery, UserDto>
{
    private readonly IRepository<User> userRepository;

    public GetUsersQueryHandler(IRepository<User> userRepository)
    {
        this.userRepository = userRepository;
    }

    protected override Task<PagedDataResult<UserDto>> HandleValidated(
        GetPagedQueryBase<UserDto> request, 
        CancellationToken cancellationToken)
    {
        var totalItems = userRepository.GetQuery().Count();

        var items = userRepository.GetQuery()
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new UserDto
            {
                Email = i.Email,
                ExternalId = i.ExternalId,
                FirstName = i.FirstName,
                LastName = i.LastName,
                PhoneNumber = i.PhoneNumber,
                UserName = i.UserName,
            })
            .ToArray();

        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);
        return Task.FromResult(new PagedDataResult<UserDto>(items, totalItems, totalPages));
    }
}
