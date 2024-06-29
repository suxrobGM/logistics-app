using Logistics.Shared;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Admin.Queries;

public sealed class GetUsersQuery : SearchableQuery, IRequest<PagedResult<UserDto>>
{
}
