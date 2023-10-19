using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Tenant.Queries;

public class GetTenantRolesQuery : SearchableQuery, IRequest<PagedResponseResult<TenantRoleDto>>
{
}
