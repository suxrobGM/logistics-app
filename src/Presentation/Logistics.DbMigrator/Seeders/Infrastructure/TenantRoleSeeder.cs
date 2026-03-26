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
/// Syncs tenant role permissions for all existing tenant databases.
/// Runs on every migrator execution to ensure new permissions are applied to existing roles.
/// </summary>
internal class TenantRoleSeeder(ILogger<TenantRoleSeeder> logger) : SeederBase(logger)
{
    public override string Name => nameof(TenantRoleSeeder);
    public override SeederType Type => SeederType.Infrastructure;
    public override int Order => 50; // After DefaultTenantSeeder (40)

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();
        var masterUow = context.MasterUnitOfWork;

        var tenants = await masterUow.Repository<Tenant>()
            .GetListAsync(t => t.ConnectionString != null, cancellationToken);

        var count = 0;
        foreach (var tenant in tenants)
        {
            // Use a fresh scope per tenant to get a clean DbContext
            using var scope = context.ServiceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
            dbContext.Database.SetConnectionString(tenant.ConnectionString);
            await SyncTenantRolesAsync(dbContext, cancellationToken);
            count++;
        }

        LogCompleted(count);
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

            // Load existing claims directly from the claims table (not via navigation)
            var existingClaims = await dbContext.Set<TenantRoleClaim>()
                .Where(c => c.RoleId == existingRole.Id)
                .ToListAsync(ct);

            var requiredPermissions = TenantRolePermissions.GetPermissionsForRole(existingRole.Name)
                .Concat(TenantRolePermissions.GetBasicPermissions())
                .Distinct()
                .ToList();

            var added = 0;
            var removed = 0;

            // Add missing permissions directly to the claims DbSet
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

            // Remove stale permissions
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
