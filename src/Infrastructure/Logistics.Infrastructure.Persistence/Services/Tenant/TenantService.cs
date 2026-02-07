using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Exceptions;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Infrastructure.Persistence;
using Logistics.Infrastructure.Persistence.Options;
using Logistics.Shared.Identity.Claims;
using Microsoft.AspNetCore.Http;

namespace Logistics.Infrastructure.Services;

internal class TenantService(
    IMasterUnitOfWork masterUow,
    TenantDbContextOptions? dbContextContextOptions = null,
    IHttpContextAccessor? contextAccessor = null)
    : ITenantService
{
    private const string TenantHeader = "X-Tenant";

    private readonly HttpContext? httpContext = contextAccessor?.HttpContext;
    private Tenant? cachedTenant;

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
        if (cachedTenant is not null)
        {
            return cachedTenant;
        }

        // No HttpContext (e.g., background worker): return default/local tenant
        if (httpContext is null)
        {
            return cachedTenant = CreateDefaultTenant();
        }

        var tenantId = ResolveTenantIdFromHttpContext();
        var tenant = await FindTenantAsync(tenantId, ct);

        CheckSubscription(tenant);

        return cachedTenant = tenant ?? throw new InvalidTenantException(
            $"Could not find tenant with ID/name '{tenantId}'.");
    }

    private async Task<Tenant?> FindTenantAsync(string tenantId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(tenantId);

        if (Guid.TryParse(tenantId, out var guid))
        {
            return await masterUow
                .Repository<Tenant>()
                .GetAsync(t => t.Id == guid, ct);
        }

        var normalized = tenantId.Trim().ToLowerInvariant();
        return await masterUow
            .Repository<Tenant>()
            .GetAsync(t => t.Name == normalized, ct);
    }

    private string ResolveTenantIdFromHttpContext()
    {
        // 1) Header
        var headerValue = httpContext!.Request.Headers[TenantHeader].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(headerValue))
        {
            return headerValue;
        }

        // 2) Claim
        var claimValue = httpContext.User.Claims
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
            ConnectionString = dbContextContextOptions?.ConnectionString
                               ?? ConnectionStrings.LocalDefaultTenant
        };
    }

    /// <summary>
    ///     Subscriptions are skipped for specific endpoints (e.g., onboarding & billing setup).
    /// </summary>
    private bool ShouldBypassSubscriptionCheck()
    {
        var path = httpContext?.Request.Path;

        if (!path.HasValue)
        {
            return false;
        }

        return path.Value.Value.StartsWith("/payments/methods", StringComparison.OrdinalIgnoreCase)
               || path.Value.Value.StartsWith("/subscriptions", StringComparison.OrdinalIgnoreCase);
    }

    private void CheckSubscription(Tenant? tenant)
    {
        if (tenant is null || !tenant.IsSubscriptionRequired || ShouldBypassSubscriptionCheck())
        {
            return;
        }

        if (tenant.Subscription is null)
        {
            throw new SubscriptionExpiredException(
                $"Tenant '{tenant.Name}' does not have a subscription.");
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
