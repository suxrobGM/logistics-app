using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Roles.Queries;

public class GetAppRolesQuery : SearchableQuery, IQuery<PagedResult<RoleDto>>
{
}
