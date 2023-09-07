using Logistics.Models;

namespace Logistics.Application.Admin.Queries;

internal sealed class GetUsersHandler : RequestHandler<GetUsersQuery, PagedResponseResult<UserDto>>
{
    private readonly IMainRepository _repository;

    public GetUsersHandler(
        IMainRepository repository)
    {
        _repository = repository;
    }

    protected override Task<PagedResponseResult<UserDto>> HandleValidated(
        GetUsersQuery req, 
        CancellationToken cancellationToken)
    {
        var totalItems = _repository.Query<User>().Count();

        var users = _repository
            .ApplySpecification(new SearchUsers(req.Search))
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
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

        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return Task.FromResult(new PagedResponseResult<UserDto>(users, totalItems, totalPages));
    }
}
