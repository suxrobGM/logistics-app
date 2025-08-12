using Logistics.Application.Abstractions;
using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetUsersHandler : RequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    private readonly IMasterUnitOfWork _masterUow;

    public GetUsersHandler(IMasterUnitOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    public override async Task<PagedResult<UserDto>> Handle(
        GetUsersQuery req,
        CancellationToken ct)
    {
        var totalItems = await _masterUow.Repository<User>().CountAsync();

        var users = _masterUow.Repository<User>()
            .ApplySpecification(new SearchUsers(req.Search, req.OrderBy, req.Page, req.PageSize))
            .Select(i => i.ToDto(null))
            .ToArray();

        return PagedResult<UserDto>.Succeed(users, totalItems, req.PageSize);
    }
}
