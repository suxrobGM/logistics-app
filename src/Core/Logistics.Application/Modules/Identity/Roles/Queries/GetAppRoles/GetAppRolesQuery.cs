using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Identity.Roles.Queries;

public class GetAppRolesQuery : SearchableQuery, IQuery<PagedResult<RoleDto>>
{
}
