using Logistics.Application.Abstractions.Common;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Admins.Queries;

/// <summary>
/// Lists users that hold an app-level role (SuperAdmin or Admin).
/// </summary>
public sealed class GetAdminsQuery : SearchableQuery, IQuery<PagedResult<UserDto>>
{
}
