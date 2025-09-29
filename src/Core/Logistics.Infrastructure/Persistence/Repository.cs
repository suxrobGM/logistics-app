using System.Linq.Expressions;
using Logistics.Domain.Core;
using Logistics.Domain.Persistence;
using Logistics.Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.Persistence;

public class Repository<TDbContext, TEntity, TEntityKey> : IRepository<TEntity, TEntityKey>
    where TEntity : class, IEntity<TEntityKey>
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;

    protected Repository(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
    {
        return _dbContext.Set<TEntity>().ApplySpecification(specification);
    }

    public IQueryable<TEntity> Query()
    {
        return _dbContext.Set<TEntity>();
    }

    public Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default)
    {
        if (predicate is null)
        {
            return _dbContext.Set<TEntity>().CountAsync(ct);
        }

        return _dbContext.Set<TEntity>().CountAsync(predicate, ct);
    }

    public Task<TEntity?> GetByIdAsync(TEntityKey id, CancellationToken ct = default)
    {
        return _dbContext.Set<TEntity>().FindAsync(id).AsTask();
    }

    public Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
    {
        return _dbContext.Set<TEntity>().FirstOrDefaultAsync(predicate, ct);
    }

    public Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
    {
        return _dbContext.Set<TEntity>()
            .Where(predicate)
            .ToListAsync(ct);
    }

    public Task<List<TEntity>> GetListAsync(ISpecification<TEntity>? specification = null,
        CancellationToken ct = default)
    {
        if (specification is null)
        {
            return _dbContext.Set<TEntity>().ToListAsync(ct);
        }

        return _dbContext.Set<TEntity>()
            .ApplySpecification(specification)
            .ToListAsync(ct);
    }

    public Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        return _dbContext.Set<TEntity>().AddAsync(entity, ct).AsTask();
    }

    public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
    {
        return _dbContext.Set<TEntity>().AddRangeAsync(entities, ct);
    }

    public void Update(TEntity entity)
    {
        _dbContext.Set<TEntity>().Update(entity);
    }

    public void Delete(TEntity? entity)
    {
        if (entity is null)
        {
            return;
        }

        _dbContext.Set<TEntity>().Remove(entity);
    }
}
