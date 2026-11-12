using Logistics.Domain.Core;
using Logistics.Domain.Persistence;
using Logistics.Infrastructure.Persistence.Data;
using Logistics.Infrastructure.Persistence.Repositories;

namespace Logistics.Infrastructure.Persistence;

internal class TenantRepository<TEntity, TKey>(TenantDbContext tenantDbContext) :
    Repository<TenantDbContext, TEntity, TKey>(tenantDbContext), ITenantRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>, ITenantEntity
{
}
