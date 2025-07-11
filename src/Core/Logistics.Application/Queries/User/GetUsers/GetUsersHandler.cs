using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetUsersHandler : RequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    private readonly IMasterUnityOfWork _masterUow;

    public GetUsersHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<PagedResult<UserDto>> HandleValidated(
        GetUsersQuery req, 
        CancellationToken cancellationToken)
    {
        var totalItems = await _masterUow.Repository<User>().CountAsync();

        var users = _masterUow.Repository<User>()
            .ApplySpecification(new SearchUsers(req.Search, req.OrderBy, req.Page, req.PageSize))
            .Select(i => i.ToDto(null))
            .ToArray();
        
        return PagedResult<UserDto>.Succeed(users, totalItems, req.PageSize);
    }
}
