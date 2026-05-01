using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;
using Logistics.Infrastructure.Persistence.Data;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Identity.Roles;
using Microsoft.EntityFrameworkCore;
using CustomClaimTypes = Logistics.Shared.Identity.Claims.CustomClaimTypes;

namespace Logistics.DbMigrator.Seeders.Infrastructure;

/// <summary>
/// Syncs tenant role permissions for the current tenant DB.
/// Runs per tenant via the orchestrator's per-tenant loop, ensuring new permissions
/// are applied to existing roles on every migrator execution.
/// </summary>
internal class TenantRoleSeeder(ILogger<TenantRoleSeeder> logger) : SeederBase(logger)
{
    public override string Name => nameof(TenantRoleSeeder);
    public override SeederType Type => SeederType.Infrastructure;
    public override int Order => 60; // After DemoTenantsSeeder (40) + RenameLegacyDemoDataSeeder (50)
    public override bool IsTenantScoped => true;

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        if (context.CurrentTenant is null)
        {
            return;
        }

        LogStarting();

        var dbContext = context.ServiceProvider.GetRequiredService<TenantDbContext>();
        await SyncTenantRolesAsync(dbContext, cancellationToken);

        LogCompleted();
    }

    private async Task SyncTenantRolesAsync(TenantDbContext dbContext, CancellationToken ct)
    {
        var roles = TenantRoles.GetValues();

        foreach (var tenantRole in roles)
        {
            var existingRole = await dbContext.Set<TenantRole>()
                .AsTracking()
                .FirstOrDefaultAsync(r => r.Name == tenantRole.Value, ct);

            if (existingRole is null)
            {
                var role = new TenantRole(tenantRole.Value) { DisplayName = tenantRole.DisplayName };
                var permissions = TenantRolePermissions.GetBasicPermissions()
                    .Concat(TenantRolePermissions.GetPermissionsForRole(role.Name))
                    .Distinct();

                foreach (var permission in permissions)
                {
                    dbContext.Set<TenantRoleClaim>().Add(
                        new TenantRoleClaim(CustomClaimTypes.Permission, permission) { RoleId = role.Id });
                }

                dbContext.Set<TenantRole>().Add(role);
                logger.LogInformation("Created tenant role '{Role}'", role.Name);
                continue;
            }

            var existingClaims = await dbContext.Set<TenantRoleClaim>()
                .Where(c => c.RoleId == existingRole.Id)
                .ToListAsync(ct);

            var requiredPermissions = TenantRolePermissions.GetPermissionsForRole(existingRole.Name)
                .Concat(TenantRolePermissions.GetBasicPermissions())
                .Distinct()
                .ToList();

            var added = 0;
            var removed = 0;

            foreach (var permission in requiredPermissions)
            {
                if (!existingClaims.Any(c =>
                    c.ClaimType == CustomClaimTypes.Permission && c.ClaimValue == permission))
                {
                    dbContext.Set<TenantRoleClaim>().Add(
                        new TenantRoleClaim(CustomClaimTypes.Permission, permission)
                        {
                            RoleId = existingRole.Id
                        });
                    added++;
                }
            }

            var claimsToRemove = existingClaims
                .Where(c => c.ClaimType == CustomClaimTypes.Permission
                    && !requiredPermissions.Contains(c.ClaimValue))
                .ToList();

            foreach (var claim in claimsToRemove)
            {
                dbContext.Set<TenantRoleClaim>().Remove(claim);
                removed++;
            }

            if (added > 0 || removed > 0)
            {
                logger.LogInformation(
                    "Synced tenant role '{Role}': +{Added} -{Removed} permissions",
                    existingRole.Name, added, removed);
            }
        }

        await dbContext.SaveChangesAsync(ct);
    }
}
