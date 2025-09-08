using Logistics.Application.Services;
using Logistics.Domain.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.Persistence;

internal sealed class TenantUnitOfWork
    : UnitOfWork<ITenantEntity>, ITenantUnitOfWork
{
    private readonly TenantDbContext _db;
    private readonly IServiceProvider _services;
    private readonly ITenantService _tenantService;
    private Tenant? _currentTenant;

    public TenantUnitOfWork(TenantDbContext db, ITenantService tenantService, IServiceProvider services) :
        base(db)
    {
        _db = db;
        _tenantService = tenantService;
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
        if (_currentTenant is null)
        {
            _currentTenant = _tenantService.GetCurrentTenant();
            _db.SwitchToTenant(_currentTenant);
        }

        return _currentTenant;
    }

    public async Task<Tenant> SetCurrentTenantByIdAsync(Guid tenantId)
    {
        var tenant = await _tenantService.FindTenantByIdAsync(tenantId.ToString());
        _currentTenant = tenant ?? throw new InvalidOperationException($"Tenant with ID '{tenantId}' not found");

        _db.SwitchToTenant(_currentTenant);
        return _currentTenant;
    }

    public void SetCurrentTenant(Tenant tenant)
    {
        _currentTenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
        _db.SwitchToTenant(_currentTenant);
    }

    protected override IRepository<TEntity, TKey> CreateRepository<TEntity, TKey>()
    {
        // First set the current tenant if not already set
        _ = GetCurrentTenant();

        // TenantRepository<TEntity, TKey>(TenantDbContext db)
        return ActivatorUtilities.CreateInstance<TenantRepository<TEntity, TKey>>(_services, _db);
    }
}
