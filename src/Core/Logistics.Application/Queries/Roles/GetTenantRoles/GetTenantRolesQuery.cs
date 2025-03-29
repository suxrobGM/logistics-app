using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class GetTenantRolesQuery : SearchableQuery, IRequest<PagedResult<RoleDto>>
{
}
