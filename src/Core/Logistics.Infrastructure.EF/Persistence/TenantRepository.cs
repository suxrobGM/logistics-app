using Logistics.Domain.Core;
using Logistics.Domain.Persistence;
using Logistics.Infrastructure.EF.Data;

namespace Logistics.Infrastructure.EF.Persistence;

public class TenantRepository<TEntity> : Repository<TenantDbContext, TEntity, string>, ITenantRepository<TEntity> 
    where TEntity : class, ITenantEntity
{
    public TenantRepository(TenantDbContext tenantDbContext) : base(tenantDbContext)
    {
    }
}
