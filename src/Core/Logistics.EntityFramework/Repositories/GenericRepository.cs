using System.Linq.Expressions;

namespace Logistics.EntityFramework.Repositories;

internal class GenericRepository<TContext> : IRepository
    where TContext : DbContext
{
    protected readonly TContext Context;

    public GenericRepository(
        TContext context,
        IUnitOfWork unitOfWork)
    {
        Context = context;
        UnitOfWork = unitOfWork;
    }

    public IUnitOfWork UnitOfWork { get; }
    
    public Task<int> CountAsync<TEntity>(Expression<Func<TEntity, bool>>? predicate = default)
        where TEntity: class, IAggregateRoot
    {
        return predicate == default ? Context.Set<TEntity>().CountAsync()
            : Context.Set<TEntity>().CountAsync(predicate);
    }

    public Task<double> SumAsync<TEntity>(Expression<Func<TEntity, double>> selector)
        where TEntity : class, IAggregateRoot
    {
        return Context.Set<TEntity>().SumAsync(selector);
    }

    public async Task<TEntity?> GetAsync<TEntity>(object? id)
        where TEntity : class, IAggregateRoot
    {
        var entity = await Context.Set<TEntity>().FindAsync(id);
        return entity;
    }

    public Task<TEntity?> GetAsync<TEntity>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : class, IAggregateRoot
    {
        return Context.Set<TEntity>().FirstOrDefaultAsync(predicate);
    }

    public Task<List<TEntity>> GetListAsync<TEntity>(Expression<Func<TEntity, bool>>? predicate = default)
        where TEntity : class, IAggregateRoot
    {
        return predicate == default ? Context.Set<TEntity>().ToListAsync()
            : Context.Set<TEntity>().Where(predicate).ToListAsync();
    }

    public Task<Dictionary<TKey, TEntity>> GetDictionaryAsync<TKey, TEntity>(
        Func<TEntity, TKey> keySelector, 
        Expression<Func<TEntity, bool>>? predicate = default) 
        where TEntity : class, IAggregateRoot 
        where TKey : notnull
    {
        return predicate == default ? Context.Set<TEntity>().ToDictionaryAsync(keySelector)
            : Context.Set<TEntity>().Where(predicate).ToDictionaryAsync(keySelector);
    }

    public IQueryable<TEntity> Query<TEntity>(Expression<Func<TEntity, bool>>? predicate = default)
        where TEntity : class, IAggregateRoot
    {
        return predicate == default
            ? Context.Set<TEntity>()
            : Context.Set<TEntity>().Where(predicate);
    }

    public async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class, IAggregateRoot
    {
        await Context.Set<TEntity>().AddAsync(entity);
    }

    public void Update<TEntity>(TEntity entity)
        where TEntity : class, IAggregateRoot
    {
        Context.Set<TEntity>().Attach(entity);
        Context.Entry(entity).State = EntityState.Modified;
    }

    public void Delete<TEntity>(TEntity? entity)
        where TEntity : class, IAggregateRoot
    {
        if (entity == null)
            return;

        Context.Set<TEntity>().Remove(entity);
    }
}