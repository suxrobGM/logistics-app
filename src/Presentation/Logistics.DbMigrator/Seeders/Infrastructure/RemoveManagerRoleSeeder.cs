using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.Shared.Identity.Roles;

namespace Logistics.DbMigrator.Seeders.Infrastructure;

/// <summary>
/// Removes the obsolete app-level "Manager" role (<c>app.manager</c>). Any users that still
/// hold it are promoted to <see cref="AppRoles.Admin"/> so they don't lose access, then the
/// role itself is deleted. Idempotent: a no-op once the role is gone.
/// </summary>
internal class RemoveManagerRoleSeeder(ILogger<RemoveManagerRoleSeeder> logger) : SeederBase(logger)
{
    private const string ObsoleteManagerRole = "app.manager";

    public override string Name => nameof(RemoveManagerRoleSeeder);
    public override SeederType Type => SeederType.Infrastructure;
    public override int Order => 15;
    public override IReadOnlyList<string> DependsOn => [nameof(AppRoleSeeder)];

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();
        var roleManager = context.RoleManager;
        var userManager = context.UserManager;

        var role = await roleManager.FindByNameAsync(ObsoleteManagerRole);
        if (role is null)
        {
            logger.LogInformation("Obsolete role '{Role}' not found; nothing to remove", ObsoleteManagerRole);
            LogCompleted();
            return;
        }

        // Promote any remaining members to Admin so they don't lose access.
        var members = await userManager.GetUsersInRoleAsync(ObsoleteManagerRole);
        foreach (var user in members)
        {
            if (!await userManager.IsInRoleAsync(user, AppRoles.Admin))
            {
                await userManager.AddToRoleAsync(user, AppRoles.Admin);
                logger.LogInformation("Promoted '{User}' from '{Old}' to '{New}'",
                    user.Email, ObsoleteManagerRole, AppRoles.Admin);
            }

            await userManager.RemoveFromRoleAsync(user, ObsoleteManagerRole);
        }

        var result = await roleManager.DeleteAsync(role);
        if (result.Succeeded)
        {
            logger.LogInformation("Deleted obsolete role '{Role}'", ObsoleteManagerRole);
        }
        else
        {
            logger.LogError("Failed to delete role '{Role}': {Errors}",
                ObsoleteManagerRole, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        LogCompleted();
    }
}
