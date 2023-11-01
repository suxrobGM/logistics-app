using System.Linq.Expressions;
using Logistics.Domain.Core;
using Logistics.Domain.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.EF.Persistence;

internal class GenericRepository<TContext> : IRepository
    where TContext : DbContext
{
    protected readonly TContext Context;

    protected GenericRepository(
        TContext context,
        IUnitOfWork unitOfWork)
    {
        Context = context;
        UnitOfWork = unitOfWork;
    }

    public IUnitOfWork UnitOfWork { get; }
    
    public Task<int> CountAsync<TEntity>(Expression<Func<TEntity, bool>>? predicate = default)
        where TEntity: class, IEntity<string>
    {
        return predicate == default ? Context.Set<TEntity>().CountAsync()
            : Context.Set<TEntity>().CountAsync(predicate);
    }

    public async Task<TEntity?> GetAsync<TEntity>(object? id)
        where TEntity : class, IEntity<string>
    {
        var entity = await Context.Set<TEntity>().FindAsync(id);
        return entity;
    }

    public Task<TEntity?> GetAsync<TEntity>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : class, IEntity<string>
    {
        return Context.Set<TEntity>().FirstOrDefaultAsync(predicate);
    }

    public Task<List<TEntity>> GetListAsync<TEntity>(Expression<Func<TEntity, bool>>? predicate = default)
        where TEntity : class, IEntity<string>
    {
        return predicate == default ? Context.Set<TEntity>().ToListAsync()
            : Context.Set<TEntity>().Where(predicate).ToListAsync();
    }

    public Task<Dictionary<TKey, TEntity>> GetDictionaryAsync<TKey, TEntity>(
        Func<TEntity, TKey> keySelector, 
        Expression<Func<TEntity, bool>>? predicate = default) 
        where TEntity : class, IEntity<string> 
        where TKey : notnull
    {
        return predicate == default ? Context.Set<TEntity>().ToDictionaryAsync(keySelector)
            : Context.Set<TEntity>().Where(predicate).ToDictionaryAsync(keySelector);
    }

    public IQueryable<TEntity> Query<TEntity>(Expression<Func<TEntity, bool>>? predicate = default)
        where TEntity : class, IEntity<string>
    {
        return predicate == default
            ? Context.Set<TEntity>()
            : Context.Set<TEntity>().Where(predicate);
    }

    public async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class, IEntity<string>
    {
        await Context.Set<TEntity>().AddAsync(entity);
    }

    public void Update<TEntity>(TEntity entity)
        where TEntity : class, IEntity<string>
    {
        Context.Set<TEntity>().Update(entity);
    }

    public void Delete<TEntity>(TEntity? entity)
        where TEntity : class, IEntity<string>
    {
        if (entity is null)
            return;

        Context.Set<TEntity>().Remove(entity);
    }
}
