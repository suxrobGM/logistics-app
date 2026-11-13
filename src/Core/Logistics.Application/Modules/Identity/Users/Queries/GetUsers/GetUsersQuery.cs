using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Identity.Users.Queries;

public sealed class GetUsersQuery : SearchableQuery, IQuery<PagedResult<UserDto>>
{
}
