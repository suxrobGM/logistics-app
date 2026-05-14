using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetTenantRolesQuery : SearchableQuery, IQuery<PagedResult<RoleDto>>
{
}
