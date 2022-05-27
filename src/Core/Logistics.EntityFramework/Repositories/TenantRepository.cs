using System.Linq.Expressions;

namespace Logistics.EntityFramework.Repositories;

internal class TenantRepository<TEntity> : GenericRepository<TEntity, TenantDbContext>, ITenantRepository<TEntity>
    where TEntity : class, IAggregateRoot, ITenantEntity
{
    public TenantRepository(
        TenantDbContext context,
        ITenantUnitOfWork unitOfWork) : base(context)
    {
        UnitOfWork = unitOfWork;
    }

    public ITenantUnitOfWork UnitOfWork { get; }
}