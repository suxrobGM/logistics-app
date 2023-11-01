using System.Linq.Expressions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Infrastructure.EF.Data;
using Microsoft.EntityFrameworkCore;

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

    public Tenant GetCurrentTenant()
    {
        ThrowIfTenantServiceIsNull();
        return _tenantContext.TenantService!.GetTenant();
    }

    public void SetCurrentTenantById(string tenantId)
    {
        ThrowIfTenantServiceIsNull();
        _tenantContext.TenantService!.SetTenantById(tenantId);
    }

    public void SetCurrentTenant(Tenant tenant)
    {
        ThrowIfTenantServiceIsNull();
        _tenantContext.TenantService!.SetTenant(tenant);
    }

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
        Context.Set<TEntity>().Update(entity);
    }

    void ITenantRepository.Delete<TEntity>(TEntity? entity)
        where TEntity : class
    {
        if (entity == null)
            return;

        Context.Set<TEntity>().Remove(entity);
    }

    private void ThrowIfTenantServiceIsNull()
    {
        if (_tenantContext.TenantService is null)
        {
            throw new InvalidOperationException("The tenant service is null from the Tenant DB context");
        }
    }
}
