using System.Security.Claims;
using Logistics.Domain.Entities;
using Logistics.Domain.Exceptions;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Infrastructure.Persistence;
using Logistics.Infrastructure.Persistence.Options;
using Logistics.Shared.Identity.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Tenancy;

namespace Logistics.Infrastructure.Persistence.Services;

internal class CurrentTenantAccessor(
    IMasterUnitOfWork masterUow,
    TenantDbContextOptions? dbContextContextOptions = null,
    IHttpContextAccessor? contextAccessor = null,
    ILogger<CurrentTenantAccessor>? logger = null)
    : ICurrentTenantAccessor
{
    private const string TenantHeader = "X-Tenant";

    private readonly HttpContext? httpContext = contextAccessor?.HttpContext;
    private Tenant? cachedTenant;

    public Tenant GetCurrentTenant()
    {
        return GetCurrentTenantAsync().GetAwaiter().GetResult();
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

        if (tenant is null)
        {
            throw new InvalidTenantException($"Could not find tenant with ID/name '{tenantId}'.");
        }

        await EnsureAuthenticatedUserHasAccessAsync(tenant, ct);

        CheckSubscription(tenant);

        return cachedTenant = tenant;
    }

    /// <summary>
    /// The <see cref="TenantHeader"/> lets multi-tenant users switch context, so a resolved tenant
    /// that differs from the caller's own claim needs an active <see cref="UserTenantAccess"/> row.
    /// Anonymous requests (webhooks, MCP) carry no tenant claim and are secured by signature or
    /// API key instead.
    /// </summary>
    private async Task EnsureAuthenticatedUserHasAccessAsync(Tenant tenant, CancellationToken ct)
    {
        var claimTenant = httpContext!.User.Claims
            .FirstOrDefault(c => c.Type == CustomClaimTypes.Tenant)?.Value;

        // No tenant claim => not an authenticated end-user request (anonymous webhook / MCP key).
        if (string.IsNullOrWhiteSpace(claimTenant))
        {
            return;
        }

        // Home tenant: the resolved tenant matches the caller's own claim - always allowed.
        if (Guid.TryParse(claimTenant, out var claimTenantId) && claimTenantId == tenant.Id)
        {
            return;
        }

        var userIdValue = httpContext.User.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier
                                 || c.Type == CustomClaimTypes.Subject)?.Value;

        if (Guid.TryParse(userIdValue, out var userId))
        {
            var hasAccess = await masterUow.Repository<UserTenantAccess>()
                .GetAsync(a => a.UserId == userId && a.TenantId == tenant.Id && a.IsActive, ct)
                is not null;

            if (hasAccess)
            {
                return;
            }
        }

        // Static message: naming the tenant would turn this endpoint into a GUID-to-name oracle
        // for anyone probing the X-Tenant header.
        logger?.LogWarning("Denied access to tenant {TenantId} for user {UserId}", tenant.Id, userIdValue);
        throw new TenantAccessDeniedException("You do not have access to this tenant.");
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
        // 0) MCP API key context (set by ApiKeyAuthenticationHandler)
        if (httpContext!.Items.TryGetValue("McpTenantId", out var mcpTenantId)
            && mcpTenantId is Guid tenantGuid)
        {
            return tenantGuid.ToString();
        }

        // 1) Header
        var headerValue = httpContext.Request.Headers[TenantHeader].FirstOrDefault();
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
        // Note: "us" matches the demo seed tenant slug; the runtime fallback resolves
        // to that DB via TenantDbContextOptions.ConnectionString.
        return new Tenant
        {
            Name = "us",
            BillingEmail = "test@test.com",
            CompanyAddress = new Address
            {
                Line1 = "7 Allstate Rd",
                City = "Dorchester",
                State = "MA",
                ZipCode = "02125",
                Country = "US"
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
