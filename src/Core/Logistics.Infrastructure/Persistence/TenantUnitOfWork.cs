﻿using System.Collections;
using Logistics.Domain.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.Persistence;

internal class TenantUnitOfWork : ITenantUnityOfWork
{
    private readonly TenantDbContext _tenantDbContext;
    private readonly Hashtable _repositories = new();

    public TenantUnitOfWork(TenantDbContext tenantDbContext)
    {
        _tenantDbContext = tenantDbContext;
    }

    public ITenantRepository<TEntity, Guid> Repository<TEntity>() 
        where TEntity : class, IEntity<Guid>, ITenantEntity
    {
        return Repository<TEntity, Guid>();
    }

    public ITenantRepository<TEntity, TKey> Repository<TEntity, TKey>() 
        where TEntity : class, IEntity<TKey>, ITenantEntity
    {
        var type = typeof(TEntity).Name;

        if (!_repositories.ContainsKey(type))
        {
            var repositoryType = typeof(TenantRepository<,>);

            var repositoryInstance =
                Activator.CreateInstance(repositoryType
                    .MakeGenericType(typeof(TEntity), typeof(TKey)), 
                    _tenantDbContext);

            _repositories.Add(type, repositoryInstance);
        }

        if (_repositories[type] is not TenantRepository<TEntity, TKey> repository)
        {
            throw new InvalidOperationException("Could not create a tenant repository");
        }
        
        return repository;
    }

    public Task<int> SaveChangesAsync()
    {
        return _tenantDbContext.SaveChangesAsync();
    }
    
    public void Dispose()
    {
        _tenantDbContext.Dispose();
    }
    
    public Tenant GetCurrentTenant()
    {
        ThrowIfTenantServiceIsNull();
        return _tenantDbContext.TenantService!.GetTenant();
    }

    public void SetCurrentTenantById(string tenantId)
    {
        ThrowIfTenantServiceIsNull();
        _tenantDbContext.TenantService!.SetTenantById(tenantId);
    }

    public void SetCurrentTenant(Tenant tenant)
    {
        ThrowIfTenantServiceIsNull();
        _tenantDbContext.TenantService!.SetTenant(tenant);
    }
    
    private void ThrowIfTenantServiceIsNull()
    {
        if (_tenantDbContext.TenantService is null)
        {
            throw new InvalidOperationException("The tenant service is null from the Tenant DB context");
        }
    }

    public async Task ExecuteRawSql(string sql)
    {
        await _tenantDbContext.Database.ExecuteSqlRawAsync(sql);
    }
    public Task<List<TSqlResponse>> ExecuteRawSql<TSqlResponse>(string sql) where TSqlResponse : class
    {
        return _tenantDbContext.Set<TSqlResponse>().FromSqlRaw(sql).ToListAsync();
    }
    public Task<List<TSqlResponse>> ExecuteRawSql<TSqlResponse>(FormattableString sql) where TSqlResponse : class
    {
        return _tenantDbContext.Set<TSqlResponse>().FromSqlInterpolated(sql).ToListAsync();
    }
}
