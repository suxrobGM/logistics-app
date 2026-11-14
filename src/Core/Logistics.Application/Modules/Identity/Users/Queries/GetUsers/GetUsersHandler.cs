using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Identity.Users.Queries;

internal sealed class GetUsersHandler(IMasterUnitOfWork masterUow)
    : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    private static readonly SortFields<User> Sorts = new SortFields<User>("email")
        .Add("email", x => x.Email);

    public async Task<PagedResult<UserDto>> Handle(
        GetUsersQuery req,
        CancellationToken ct)
    {
        var query = masterUow.Repository<User>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            var search = req.Search.ToLower();
            query = query.Where(i =>
                i.FirstName.ToLower().Contains(search) ||
                i.LastName.ToLower().Contains(search) ||
                (i.PhoneNumber != null && i.PhoneNumber.ToLower().Contains(search)) ||
                (i.Email != null && i.Email.ToLower().Contains(search)));
        }

        return await query.ToPagedResultAsync(req, Sorts, i => i.ToDto(), ct);
    }
}
