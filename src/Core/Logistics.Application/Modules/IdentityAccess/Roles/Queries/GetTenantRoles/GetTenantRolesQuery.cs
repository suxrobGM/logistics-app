using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Roles.Queries;

public class GetTenantRolesQuery : SearchableQuery, IQuery<PagedResult<RoleDto>>
{
}
