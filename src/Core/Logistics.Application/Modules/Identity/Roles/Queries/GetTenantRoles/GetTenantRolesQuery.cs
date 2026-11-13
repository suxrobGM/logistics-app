using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Identity.Roles.Queries;

public class GetTenantRolesQuery : SearchableQuery, IQuery<PagedResult<RoleDto>>
{
}
