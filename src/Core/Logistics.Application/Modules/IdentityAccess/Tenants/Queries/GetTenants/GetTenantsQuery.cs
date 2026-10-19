using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Queries;

public sealed class GetTenantsQuery : SearchableQuery, IQuery<PagedResult<TenantDto>>
{
}
