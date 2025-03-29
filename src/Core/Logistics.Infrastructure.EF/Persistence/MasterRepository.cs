using Logistics.Domain.Core;
using Logistics.Domain.Persistence;
using Logistics.Infrastructure.EF.Data;

namespace Logistics.Infrastructure.EF.Persistence;

public class MasterRepository<TEntity, TKey> : 
    Repository<MasterDbContext, TEntity, TKey>, IMasterRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    public MasterRepository(MasterDbContext masterDbContext) : base(masterDbContext)
    {
    }
}
