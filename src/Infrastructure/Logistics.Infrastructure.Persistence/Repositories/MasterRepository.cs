using Logistics.Domain.Core;
using Logistics.Domain.Persistence;
using Logistics.Infrastructure.Persistence.Data;

namespace Logistics.Infrastructure.Persistence.Repositories;

internal class MasterRepository<TEntity, TKey> :
    Repository<MasterDbContext, TEntity, TKey>, IMasterRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>, IMasterEntity
{
    public MasterRepository(MasterDbContext masterDbContext) : base(masterDbContext)
    {
    }
}
