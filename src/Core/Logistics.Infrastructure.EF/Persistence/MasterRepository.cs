using Logistics.Domain.Core;
using Logistics.Domain.Persistence;
using Logistics.Infrastructure.EF.Data;

namespace Logistics.Infrastructure.EF.Persistence;

public class MasterRepository<TEntity> : Repository<MasterDbContext, TEntity, string>, IMasterRepository<TEntity>
    where TEntity : class, IEntity<string>
{
    public MasterRepository(MasterDbContext masterDbContext) : base(masterDbContext)
    {
    }
}
