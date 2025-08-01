using Logistics.Domain.Core;
using Logistics.Domain.Persistence;
using Logistics.Infrastructure.Data;

namespace Logistics.Infrastructure.Persistence;

internal class MasterRepository<TEntity, TKey> : 
    Repository<MasterDbContext, TEntity, TKey>, IMasterRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>, IMasterEntity
{
    public MasterRepository(MasterDbContext masterDbContext) : base(masterDbContext)
    {
    }
}
