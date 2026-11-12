using Logistics.Domain.Core;
using Logistics.Domain.Persistence;
using Logistics.Infrastructure.Persistence.Data;

namespace Logistics.Infrastructure.Persistence.Repositories;

internal class MasterRepository<TEntity, TKey>(MasterDbContext masterDbContext) :
    Repository<MasterDbContext, TEntity, TKey>(masterDbContext), IMasterRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>, IMasterEntity
{
}
