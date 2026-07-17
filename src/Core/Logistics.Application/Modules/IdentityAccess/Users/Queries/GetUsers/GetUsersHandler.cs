using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Users.Queries;

internal sealed class GetUsersHandler(IMasterUnitOfWork masterUow)
    : IAppRequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    public Task<PagedResult<UserDto>> Handle(
        GetUsersQuery req,
        CancellationToken ct)
    {
        var query = masterUow.Repository<User>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            query = query.Where(i =>
                i.FirstName.Contains(req.Search) ||
                i.LastName.Contains(req.Search) ||
                (i.PhoneNumber != null && i.PhoneNumber.Contains(req.Search)) ||
                (i.Email != null && i.Email.Contains(req.Search)));
        }

        var totalItems = query.Count();

        var users = query
            .OrderBy(req.OrderBy)
            .ApplyPaging(req.Page, req.PageSize)
            .Select(i => i.ToDto())
            .ToArray();

        return Task.FromResult(PagedResult<UserDto>.Ok(users, totalItems, req.PageSize));
    }
}
