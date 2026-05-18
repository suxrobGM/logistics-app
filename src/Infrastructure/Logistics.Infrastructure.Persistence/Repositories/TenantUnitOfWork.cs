using Logistics.Domain.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Infrastructure.Persistence.Data;
using Microsoft.Extensions.DependencyInjection;
using Logistics.Application.Abstractions.Tenancy;

namespace Logistics.Infrastructure.Persistence;

internal sealed class TenantUnitOfWork(
    TenantDbContext db,
    ICurrentTenantAccessor currentTenantAccessor,
    IMasterUnitOfWork masterUow,
    IServiceProvider services)
    : UnitOfWork<ITenantEntity>(db), ITenantUnitOfWork
{
    private Tenant? currentTenant;

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
        if (currentTenant is null)
        {
            currentTenant = currentTenantAccessor.GetCurrentTenant();
            db.SwitchToTenant(currentTenant);
        }

        return currentTenant;
    }

    public async Task<Tenant> SetCurrentTenantByIdAsync(Guid tenantId)
    {
        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(tenantId);
        currentTenant = tenant ?? throw new InvalidOperationException($"Tenant with ID '{tenantId}' not found");

        db.SwitchToTenant(currentTenant);
        return currentTenant;
    }

    public void SetCurrentTenant(Tenant tenant)
    {
        currentTenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
        db.SwitchToTenant(currentTenant);
    }

    protected override IRepository<TEntity, TKey> CreateRepository<TEntity, TKey>()
    {
        // First set the current tenant if not already set
        _ = GetCurrentTenant();

        // TenantRepository<TEntity, TKey>(TenantDbContext db)
        return ActivatorUtilities.CreateInstance<TenantRepository<TEntity, TKey>>(services, db);
    }
}
