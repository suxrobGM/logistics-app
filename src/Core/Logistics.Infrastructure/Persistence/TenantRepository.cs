using Logistics.Domain.Core;
using Logistics.Domain.Persistence;
using Logistics.Infrastructure.Data;

namespace Logistics.Infrastructure.Persistence;

internal class TenantRepository<TEntity, TKey> :
    Repository<TenantDbContext, TEntity, TKey>, ITenantRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>, ITenantEntity
{
    public TenantRepository(TenantDbContext tenantDbContext) : base(tenantDbContext)
    {
    }
}
