using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Queries;

public sealed class GetUsersQuery : SearchableQuery, IRequest<PagedResult<UserDto>>
{
}
