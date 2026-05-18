using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Users.Queries;

public sealed class GetUsersQuery : SearchableQuery, IQuery<PagedResult<UserDto>>
{
}
