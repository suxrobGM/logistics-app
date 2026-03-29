using System.Security.Claims;
using System.Text.Encodings.Web;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Identity.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.McpServer.Authentication;

internal sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers.Authorization.FirstOrDefault();

        if (string.IsNullOrEmpty(authHeader) ||
            !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var rawKey = authHeader["Bearer ".Length..].Trim();

        if (!rawKey.StartsWith(ApiKeyDefaults.KeyPrefix))
        {
            return AuthenticateResult.NoResult();
        }

        // Parse tenant ID from key format: logsx_{tenantId}_{random}
        if (!TryParseTenantId(rawKey, out var tenantId))
        {
            return AuthenticateResult.Fail("Invalid API key format.");
        }

        // Resolve tenant from master DB
        var masterUow = Context.RequestServices.GetRequiredService<IMasterUnitOfWork>();
        var tenant = await masterUow.Repository<Tenant>()
            .GetAsync(t => t.Id == tenantId);

        if (tenant is null)
        {
            return AuthenticateResult.Fail("Invalid API key.");
        }

        // Switch to tenant DB and validate key hash
        var tenantUow = Context.RequestServices.GetRequiredService<ITenantUnitOfWork>();
        tenantUow.SetCurrentTenant(tenant);

        var keyHash = ApiKeyHasher.Hash(rawKey);
        var apiKey = await tenantUow.Repository<ApiKey>()
            .GetAsync(k => k.KeyHash == keyHash);

        if (apiKey is null)
        {
            return AuthenticateResult.Fail("Invalid API key.");
        }

        // Set tenant context for downstream services
        Context.Items[ApiKeyDefaults.McpTenantIdKey] = tenantId;

        // Fire-and-forget LastUsedAt update
        var apiKeyId = apiKey.Id;
        UpdateLastUsedAt(apiKeyId, tenant);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, apiKey.Id.ToString()),
            new Claim(CustomClaimTypes.Tenant, tenantId.ToString()),
            new Claim("api_key_name", apiKey.Name)
        };

        var identity = new ClaimsIdentity(claims, ApiKeyDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, ApiKeyDefaults.AuthenticationScheme);

        return AuthenticateResult.Success(ticket);
    }

    private void UpdateLastUsedAt(Guid apiKeyId, Tenant tenant)
    {
        // Capture IServiceScopeFactory synchronously while HttpContext is still alive
        var scopeFactory = Context.RequestServices.GetRequiredService<IServiceScopeFactory>();
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var uow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
                uow.SetCurrentTenant(tenant);
                var key = await uow.Repository<ApiKey>().GetByIdAsync(apiKeyId);
                if (key is not null)
                {
                    key.LastUsedAt = DateTime.UtcNow;
                    await uow.SaveChangesAsync();
                }
            }
            catch
            {
                // Best-effort — don't fail auth for LastUsedAt tracking
            }
        });
    }

    private static bool TryParseTenantId(string rawKey, out Guid tenantId)
    {
        tenantId = Guid.Empty;

        // Format: logsx_{tenantId}_{random}
        var afterPrefix = rawKey[ApiKeyDefaults.KeyPrefix.Length..];
        var underscoreIndex = afterPrefix.IndexOf('_');
        if (underscoreIndex <= 0)
            return false;

        return Guid.TryParse(afterPrefix[..underscoreIndex], out tenantId);
    }
}
