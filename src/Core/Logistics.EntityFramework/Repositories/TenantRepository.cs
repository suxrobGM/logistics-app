using System.Linq.Expressions;

namespace Logistics.EntityFramework.Repositories;

internal class TenantRepository<TEntity> : ITenantRepository<TEntity>
    where TEntity : class, IAggregateRoot, ITenantEntity
{
    private readonly TenantDbContext context;

    public TenantRepository(
        TenantDbContext context,
        ITenantUnitOfWork unitOfWork)
    {
        this.context = context;
        UnitOfWork = unitOfWork;
    }

    public ITenantUnitOfWork UnitOfWork { get; }

    public async Task<TEntity?> GetAsync(object id)
    {
        var entity = await context.Set<TEntity>().FindAsync(id);
        return entity;
    }

    public Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return context.Set<TEntity>().FirstOrDefaultAsync(predicate);
    }

    public async Task<IList<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate = default!)
    {
        return predicate == null ? await context.Set<TEntity>().ToListAsync()
            : await context.Set<TEntity>().Where(predicate).ToListAsync();
    }

    public IQueryable<TEntity> GetQuery(Expression<Func<TEntity, bool>> predicate = default!)
    {
        return predicate != null ? context.Set<TEntity>().Where(predicate)
            : context.Set<TEntity>();
    }

    public async Task AddAsync(TEntity entity)
    {
        await context.Set<TEntity>().AddAsync(entity);
    }

    public void Update(TEntity entity)
    {
        context.Set<TEntity>().Attach(entity);
        context.Entry(entity).State = EntityState.Modified;
    }

    public void Delete(object id)
    {
        if (id is null)
            return;

        var entity = context.Set<TEntity>().Find(id);
        Delete(entity);
    }

    public void Delete(TEntity? entity)
    {
        if (entity == null)
            return;

        context.Set<TEntity>().Remove(entity);
    }
}