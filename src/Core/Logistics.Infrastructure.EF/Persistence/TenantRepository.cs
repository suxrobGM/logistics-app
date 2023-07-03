using System.Linq.Expressions;

namespace Logistics.Infrastructure.EF.Persistence;

internal class TenantRepository : GenericRepository<TenantDbContext>, ITenantRepository
{
    private readonly TenantDbContext _tenantContext;
    
    public TenantRepository(
        TenantDbContext context,
        UnitOfWork<TenantDbContext> unitOfWork) : base(context, unitOfWork)
    {
        _tenantContext = context;
    }
    
    public Tenant? CurrentTenant => _tenantContext.CurrentTenant;
    
    async Task<TEntity?> ITenantRepository.GetAsync<TEntity>(object? id)
        where TEntity : class
    {
        var entity = await Context.Set<TEntity>().FindAsync(id);
        return entity;
    }

    Task<TEntity?> ITenantRepository.GetAsync<TEntity>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : class
    {
        return Context.Set<TEntity>().FirstOrDefaultAsync(predicate);
    }

   Task<List<TEntity>> ITenantRepository.GetListAsync<TEntity>(Expression<Func<TEntity, bool>>? predicate)
        where TEntity : class
    {
        return predicate == default ? Context.Set<TEntity>().ToListAsync()
            : Context.Set<TEntity>().Where(predicate).ToListAsync();
    }

   Task<Dictionary<TKey, TEntity>> ITenantRepository.GetDictionaryAsync<TKey, TEntity>(
       Func<TEntity, TKey> keySelector, 
       Expression<Func<TEntity, bool>>? predicate)
       where TEntity : class
   {
       return predicate == default ? Context.Set<TEntity>().ToDictionaryAsync(keySelector)
           : Context.Set<TEntity>().Where(predicate).ToDictionaryAsync(keySelector);
   }

   IQueryable<TEntity> ITenantRepository.Query<TEntity>(Expression<Func<TEntity, bool>>? predicate)
        where TEntity : class
    {
        return predicate == default
            ? Context.Set<TEntity>()
            : Context.Set<TEntity>().Where(predicate);
    }

    async Task ITenantRepository.AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        await Context.Set<TEntity>().AddAsync(entity);
    }

    void ITenantRepository.Update<TEntity>(TEntity entity)
        where TEntity : class
    {
        Context.Set<TEntity>().Attach(entity);
        Context.Entry(entity).State = EntityState.Modified;
    }

    void ITenantRepository.Delete<TEntity>(TEntity? entity)
        where TEntity : class
    {
        if (entity == null)
            return;

        Context.Set<TEntity>().Remove(entity);
    }
}