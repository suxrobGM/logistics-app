using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Tenancy.Tenants.Queries;

public sealed class GetTenantsQuery : SearchableQuery, IQuery<PagedResult<TenantDto>>
{
}
