using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Exceptions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Infrastructure.Data;
using Logistics.Infrastructure.Options;
using Logistics.Shared.Identity.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.Services;

internal class TenantService : ITenantService
{
    private const string TenantHeader = "X-Tenant";

    private readonly TenantDbContextOptions? _dbContextOptions;
    private readonly HttpContext? _httpContext;
    private readonly MasterDbContext _masterDbContext;
    private Tenant? _cachedTenant;

    public TenantService(
        MasterDbContext masterDbContext,
        TenantDbContextOptions? dbContextContextOptions = null,
        IHttpContextAccessor? contextAccessor = null)
    {
        _dbContextOptions = dbContextContextOptions;
        _httpContext = contextAccessor?.HttpContext;
        _masterDbContext = masterDbContext ?? throw new ArgumentNullException(nameof(masterDbContext));
    }

    public Tenant GetCurrentTenant()
    {
        return GetCurrentTenantAsync().GetAwaiter().GetResult();
    }

    public Task<Tenant?> FindTenantByIdAsync(string tenantId)
    {
        return FindTenantAsync(tenantId);
    }

    public async Task<Tenant> GetCurrentTenantAsync(CancellationToken ct = default)
    {
        if (_cachedTenant is not null)
        {
            return _cachedTenant;
        }

        // No HttpContext (e.g., background worker): return default/local tenant
        if (_httpContext is null)
        {
            return _cachedTenant = CreateDefaultTenant();
        }

        var tenantId = ResolveTenantIdFromHttpContext();
        var tenant = await FindTenantAsync(tenantId, ct);

        CheckSubscription(tenant);

        return _cachedTenant = tenant ?? throw new InvalidTenantException(
            $"Could not find tenant with ID/name '{tenantId}'.");
    }

    private async Task<Tenant?> FindTenantAsync(string tenantId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(tenantId);

        if (Guid.TryParse(tenantId, out var guid))
        {
            return await _masterDbContext
                .Set<Tenant>()
                .FirstOrDefaultAsync(t => t.Id == guid, ct);
        }

        var normalized = tenantId.Trim().ToLowerInvariant();
        return await _masterDbContext
            .Set<Tenant>()
            .FirstOrDefaultAsync(t => t.Name == normalized, ct);
    }

    private string ResolveTenantIdFromHttpContext()
    {
        // 1) Header
        var headerValue = _httpContext!.Request.Headers[TenantHeader].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(headerValue))
        {
            return headerValue;
        }

        // 2) Claim
        var claimValue = _httpContext.User.Claims
            .FirstOrDefault(c => c.Type == CustomClaimTypes.Tenant)?.Value;

        if (!string.IsNullOrWhiteSpace(claimValue))
        {
            return claimValue;
        }

        throw new InvalidTenantException(
            $"Tenant ID must be specified in the '{TenantHeader}' header or '{CustomClaimTypes.Tenant}' claim.");
    }

    private Tenant CreateDefaultTenant()
    {
        // Safe defaults for non-HTTP scenarios (jobs, migrations, etc.)
        return new Tenant
        {
            Name = "default",
            BillingEmail = "test@test.com",
            CompanyAddress = new Address
            {
                Line1 = "123 Main St",
                City = "Anytown",
                State = "CA",
                ZipCode = "12345",
                Country = "United States"
            },
            ConnectionString = _dbContextOptions?.ConnectionString
                               ?? ConnectionStrings.LocalDefaultTenant
        };
    }

    /// <summary>
    ///     Subscriptions are skipped for specific endpoints (e.g., onboarding & billing setup).
    /// </summary>
    private bool ShouldBypassSubscriptionCheck()
    {
        var path = _httpContext?.Request.Path;

        if (!path.HasValue)
        {
            return false;
        }

        return path.Value.Value.StartsWith("/payments/methods", StringComparison.OrdinalIgnoreCase)
               || path.Value.Value.StartsWith("/subscriptions", StringComparison.OrdinalIgnoreCase);
    }

    private void CheckSubscription(Tenant? tenant)
    {
        if (tenant?.Subscription is null || ShouldBypassSubscriptionCheck())
        {
            return;
        }

        var status = tenant.Subscription.Status;
        if (status is SubscriptionStatus.Active or SubscriptionStatus.Trialing)
        {
            return;
        }

        throw new SubscriptionExpiredException(
            $"Tenant '{tenant.Name}' does not have an active subscription. Current status: '{status}'.");
    }
}
