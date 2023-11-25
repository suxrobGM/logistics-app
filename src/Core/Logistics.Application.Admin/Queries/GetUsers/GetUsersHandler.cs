using Logistics.Application.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Specifications;
using Logistics.Shared.Models;
using Logistics.Shared;

namespace Logistics.Application.Admin.Queries;

internal sealed class GetUsersHandler : RequestHandler<GetUsersQuery, PagedResponseResult<UserDto>>
{
    private readonly IMasterUnityOfWork _masterUow;

    public GetUsersHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<PagedResponseResult<UserDto>> HandleValidated(
        GetUsersQuery req, 
        CancellationToken cancellationToken)
    {
        var totalItems = await _masterUow.Repository<User>().CountAsync();

        var users = _masterUow.Repository<User>()
            .ApplySpecification(new SearchUsers(req.Search, req.OrderBy, req.Page, req.PageSize, req.Descending))
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
        return PagedResponseResult<UserDto>.Create(users, totalItems, totalPages);
    }
}
