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

    public async Task<IList<TEntity>> GetListAsync<TEntity>(Expression<Func<TEntity, bool>>? predicate = default)
        where TEntity : class, IAggregateRoot
    {
        return predicate == default ? await Context.Set<TEntity>().ToListAsync()
            : await Context.Set<TEntity>().Where(predicate).ToListAsync();
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