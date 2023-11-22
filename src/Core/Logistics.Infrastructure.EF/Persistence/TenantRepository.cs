using System.Linq.Expressions;
using Logistics.Domain.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Specifications;
using Logistics.Infrastructure.EF.Data;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.EF.Persistence;

public class TenantRepository<TEntity> : ITenantRepository<TEntity> 
    where TEntity : class, ITenantEntity
{
    private readonly TenantDbContext _tenantDbContext;
    
    public TenantRepository(TenantDbContext tenantDbContext)
    {
        _tenantDbContext = tenantDbContext;
    }
    
    public IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
    {
        return SpecificationEvaluator<TEntity>.GetQuery(_tenantDbContext.Set<TEntity>(), specification);
    }
    
    public IQueryable<TEntity> Query()
    {
        return _tenantDbContext.Set<TEntity>();
    }
    
    public Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = default)
    {
        if (predicate is null)
        {
            return _tenantDbContext.Set<TEntity>().CountAsync();
        }
        
        return _tenantDbContext.Set<TEntity>().CountAsync(predicate);
    }

    public Task<TEntity?> GetByIdAsync(string id)
    {
        return _tenantDbContext.Set<TEntity>().FindAsync(id).AsTask();
    }

    public Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return _tenantDbContext.Set<TEntity>().FirstOrDefaultAsync(predicate);
    }

    public Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return _tenantDbContext.Set<TEntity>().Where(predicate).ToListAsync();
    }

    public Task<List<TEntity>> GetListAsync(ISpecification<TEntity>? specification = default)
    {
        if (specification is null)
        {
            return _tenantDbContext.Set<TEntity>().ToListAsync();
        }

        return SpecificationEvaluator<TEntity>.GetQuery(_tenantDbContext.Set<TEntity>(), specification).ToListAsync();
    }

    public Task AddAsync(TEntity entity)
    {
        return _tenantDbContext.Set<TEntity>().AddAsync(entity).AsTask();
    }

    public void Update(TEntity entity)
    {
        _tenantDbContext.Set<TEntity>().Update(entity);
    }

    public void Delete(TEntity? entity)
    {
        if (entity is null)
        {
            return;
        }

        _tenantDbContext.Set<TEntity>().Remove(entity);
    }
}
