using Logistics.Domain.Entities;
using Logistics.Domain.Exceptions;
using Logistics.Domain.Services;
using Logistics.Infrastructure.EF.Data;
using Logistics.Infrastructure.EF.Options;
using Logistics.Shared.Consts;
using Logistics.Shared.Consts.Claims;
using Microsoft.AspNetCore.Http;

namespace Logistics.Infrastructure.EF.Services;

internal class TenantService : ITenantService
{
    private readonly TenantDbContextOptions? _dbContextOptions;
    private readonly MasterDbContext _masterDbContext;
    private readonly HttpContext? _httpContext;
    private Tenant? _currentTenant;

    public TenantService(
        MasterDbContext masterDbContext, 
        TenantDbContextOptions? dbContextContextOptions = null,
        IHttpContextAccessor? contextAccessor = null)
    {
        _dbContextOptions = dbContextContextOptions;
        _httpContext = contextAccessor?.HttpContext;
        _masterDbContext = masterDbContext ?? throw new ArgumentNullException(nameof(masterDbContext));
    }

    public Tenant GetTenant()
    {
        if (_currentTenant is not null)
            return _currentTenant;

        if (_httpContext is null)
        {
            _currentTenant = CreateDefaultTenant();
            return _currentTenant;
        }

        var tenantId = GetTenantIdFromHttpContext();
        _currentTenant = FetchCurrentTenant(tenantId);
        CheckSubscription(_currentTenant);
        
        return _currentTenant ?? throw new InvalidTenantException(
            $"Could not find tenant with ID/name '{tenantId}'");
    }

    public Tenant? SetTenantById(string tenantId)
    {
        var tenant = FetchCurrentTenant(tenantId);
        _currentTenant = tenant;
        return _currentTenant;
    }

    public void SetTenant(Tenant tenant)
    {
        _currentTenant = tenant;
    }

    private Tenant CreateDefaultTenant()
    {
        return new Tenant
        {
            Name = "default",
            BillingEmail = "test@gmail.com",
            ConnectionString = _dbContextOptions?.ConnectionString ?? ConnectionStrings.LocalDefaultTenant,
        };
    }

    private string GetTenantIdFromHttpContext()
    {
        var tenantId = _httpContext!.Request.Headers["X-Tenant"].FirstOrDefault();
        if (!string.IsNullOrEmpty(tenantId))
            return tenantId;

        tenantId = _httpContext.User.Claims
            .FirstOrDefault(c => c.Type == CustomClaimTypes.Tenant)?.Value;
        
        if (!string.IsNullOrEmpty(tenantId))
            return tenantId;

        throw new InvalidTenantException(
            "Tenant ID must be specified in the 'X-Tenant' header or 'tenant' claim");
    }

    private Tenant? FetchCurrentTenant(string? tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new InvalidTenantException(
                "Tenant ID must be specified in the 'X-Tenant' header or 'tenant' claim");
        }

        var normalizedId = tenantId.Trim().ToLowerInvariant();
        return _masterDbContext.Set<Tenant>()
            .FirstOrDefault(t => t.Id == normalizedId || t.Name == normalizedId);
    }
    
    /// <summary>
    /// Check if the tenant has an active subscription. Throws <see cref="SubscriptionExpiredException"/> if not.
    /// If tenant subscription is null, it is free and considered active.
    /// </summary>
    /// <param name="tenant">Tenant to check</param>
    /// <exception cref="SubscriptionExpiredException"></exception>
    private static void CheckSubscription(Tenant? tenant)
    {
        if (tenant?.Subscription is null)
            return;
        
        if (tenant.Subscription.Status != SubscriptionStatus.Active)
        {
            throw new SubscriptionExpiredException(
                $"Tenant '{tenant.Name}' does not have an active subscription");
        }
    }
}
