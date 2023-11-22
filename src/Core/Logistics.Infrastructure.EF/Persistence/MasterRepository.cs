using System.Linq.Expressions;
using Logistics.Domain.Core;
using Logistics.Domain.Persistence;
using Logistics.Domain.Specifications;
using Logistics.Infrastructure.EF.Data;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.EF.Persistence;

public class MasterRepository<TEntity> : IMasterRepository<TEntity> 
    where TEntity : class, IEntity<string>
{
    private readonly MasterDbContext _masterDbContext;
    
    public MasterRepository(MasterDbContext masterDbContext)
    {
        _masterDbContext = masterDbContext;
    }

    public IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
    {
        return SpecificationEvaluator<TEntity>.GetQuery(_masterDbContext.Set<TEntity>(), specification);
    }

    public IQueryable<TEntity> Query()
    {
        return _masterDbContext.Set<TEntity>();
    }
    
    public Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = default)
    {
        if (predicate is null)
        {
            return _masterDbContext.Set<TEntity>().CountAsync();
        }
        
        return _masterDbContext.Set<TEntity>().CountAsync(predicate);
    }

    public Task<TEntity?> GetByIdAsync(string id)
    {
        return _masterDbContext.Set<TEntity>().FindAsync(id).AsTask();
    }

    public Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return _masterDbContext.Set<TEntity>().FirstOrDefaultAsync(predicate);
    }

    public Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return _masterDbContext.Set<TEntity>().Where(predicate).ToListAsync();
    }

    public Task<List<TEntity>> GetListAsync(ISpecification<TEntity>? specification = default)
    {
        if (specification is null)
        {
            return _masterDbContext.Set<TEntity>().ToListAsync();
        }

        return SpecificationEvaluator<TEntity>.GetQuery(_masterDbContext.Set<TEntity>(), specification).ToListAsync();
    }

    public Task AddAsync(TEntity entity)
    {
        return _masterDbContext.Set<TEntity>().AddAsync(entity).AsTask();
    }

    public void Update(TEntity entity)
    {
        _masterDbContext.Set<TEntity>().Update(entity);
    }

    public void Delete(TEntity? entity)
    {
        if (entity is null)
        {
            return;
        }

        _masterDbContext.Set<TEntity>().Remove(entity);
    }
}
