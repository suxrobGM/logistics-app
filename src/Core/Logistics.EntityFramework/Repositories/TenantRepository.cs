using System.Linq.Expressions;

namespace Logistics.EntityFramework.Repositories;

internal class TenantRepository<TEntity> : GenericRepository<TEntity, TenantDbContext>, ITenantRepository<TEntity>
    where TEntity : class, IAggregateRoot, ITenantEntity
{
    private readonly TenantDbContext _tenantContext;
    
    public TenantRepository(
        TenantDbContext context,
        ITenantUnitOfWork unitOfWork) : base(context)
    {
        _tenantContext = context;
        UnitOfWork = unitOfWork;
    }

    public ITenantUnitOfWork UnitOfWork { get; }
    public Tenant? CurrentTenant => _tenantContext.CurrentTenant;
}