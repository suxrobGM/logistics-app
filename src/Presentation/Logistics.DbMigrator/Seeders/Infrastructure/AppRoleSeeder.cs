using System.Security.Claims;
using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Identity.Roles;
using Microsoft.AspNetCore.Identity;
using CustomClaimTypes = Logistics.Shared.Identity.Claims.CustomClaimTypes;

namespace Logistics.DbMigrator.Seeders.Infrastructure;

/// <summary>
/// Seeds application roles with their permissions.
/// </summary>
internal class AppRoleSeeder(ILogger<AppRoleSeeder> logger) : SeederBase(logger)
{
    public override string Name => nameof(AppRoleSeeder);
    public override SeederType Type => SeederType.Infrastructure;
    public override int Order => 10;

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();
        var roleManager = context.RoleManager;
        var appRoles = AppRoles.GetValues();
        var count = 0;

        foreach (var appRole in appRoles)
        {
            var role = new AppRole(appRole.Value)
            {
                DisplayName = appRole.DisplayName
            };

            var existingRole = await roleManager.FindByNameAsync(role.Name!);
            if (existingRole is not null)
            {
                await UpdateRolePermissionsAsync(roleManager, existingRole);
                logger.LogInformation("Updated app role '{Role}'", existingRole.Name);
            }
            else
            {
                var result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    await AddPermissionsAsync(roleManager, role, AppRolePermissions.GetBasicPermissions());
                    await AddPermissionsAsync(roleManager, role, AppRolePermissions.GetPermissionsForRole(role.Name!));
                    logger.LogInformation("Created app role '{RoleName}'", role.Name);
                }
                else
                {
                    logger.LogError("Failed to create role '{RoleName}': {Errors}",
                        role.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            count++;
        }

        LogCompleted(count);
    }

    private async Task UpdateRolePermissionsAsync(RoleManager<AppRole> roleManager, AppRole role)
    {
        var currentClaims = await roleManager.GetClaimsAsync(role);
        var requiredPermissions = AppRolePermissions.GetPermissionsForRole(role.Name!)
            .Concat(AppRolePermissions.GetBasicPermissions())
            .Distinct()
            .ToList();

        // Add missing permissions
        foreach (var permission in requiredPermissions)
        {
            if (!currentClaims.Any(c => c.Type == CustomClaimTypes.Permission && c.Value == permission))
            {
                await roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, permission));
                logger.LogInformation("Added permission '{Permission}' to role '{Role}'", permission, role.Name);
            }
        }

        // Remove permissions that are no longer in the required list
        var permissionsToRemove = currentClaims
            .Where(c => c.Type == CustomClaimTypes.Permission && !requiredPermissions.Contains(c.Value))
            .ToList();

        foreach (var claim in permissionsToRemove)
        {
            await roleManager.RemoveClaimAsync(role, claim);
            logger.LogInformation("Removed permission '{Permission}' from role '{Role}'", claim.Value, role.Name);
        }
    }

    private async Task AddPermissionsAsync(RoleManager<AppRole> roleManager, AppRole role, IEnumerable<string> permissions)
    {
        foreach (var permission in permissions)
        {
            var allClaims = await roleManager.GetClaimsAsync(role);
            var claim = new Claim(CustomClaimTypes.Permission, permission);

            if (!allClaims.Any(c => c.Type == claim.Type && c.Value == claim.Value))
            {
                var result = await roleManager.AddClaimAsync(role, claim);
                if (result.Succeeded)
                {
                    logger.LogInformation("Added claim '{ClaimValue}' to role '{Role}'", claim.Value, role.Name);
                }
            }
        }
    }
}
