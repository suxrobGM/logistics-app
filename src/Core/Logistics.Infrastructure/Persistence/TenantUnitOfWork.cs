using Logistics.Domain.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Infrastructure.Data;
using Logistics.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.Persistence;

internal sealed class TenantUnitOfWork
    : UnitOfWork<ITenantEntity>, ITenantUnitOfWork
{
    private readonly TenantDbContext _db;
    private readonly IServiceProvider _services;

    public TenantUnitOfWork(TenantDbContext db, IServiceProvider services) : base(db)
    {
        _db = db;
        _services = services;
    }

    // Strongly typed repos (hide base with 'new')
    public new ITenantRepository<TEntity, Guid> Repository<TEntity>()
        where TEntity : class, IEntity<Guid>, ITenantEntity
    {
        return (ITenantRepository<TEntity, Guid>)base.Repository<TEntity>();
    }

    public new ITenantRepository<TEntity, TKey> Repository<TEntity, TKey>()
        where TEntity : class, IEntity<TKey>, ITenantEntity
    {
        return (ITenantRepository<TEntity, TKey>)base.Repository<TEntity, TKey>();
    }

    // Tenant context operations
    public Tenant GetCurrentTenant()
    {
        var tenantService = _services.GetRequiredService<TenantService>();
        return tenantService.GetTenant();
    }

    public void SetCurrentTenantById(string tenantId)
    {
        var tenantService = _services.GetRequiredService<TenantService>();
        tenantService.SetTenantById(tenantId);
    }

    public void SetCurrentTenant(Tenant tenant)
    {
        var tenantService = _services.GetRequiredService<TenantService>();
        tenantService.SetTenant(tenant);
    }

    protected override IRepository<TEntity, TKey> CreateRepository<TEntity, TKey>()
    {
        // TenantRepository<TEntity, TKey>(TenantDbContext db)
        return ActivatorUtilities.CreateInstance<TenantRepository<TEntity, TKey>>(_services, _db);
    }
}
