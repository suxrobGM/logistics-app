using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;
using Logistics.Shared.Identity.Roles;

namespace Logistics.DbMigrator.Seeders.Infrastructure;

/// <summary>
///     Seeds the super admin user from configuration.
/// </summary>
internal class SuperAdminSeeder(ILogger<SuperAdminSeeder> logger) : SeederBase(logger)
{
    public override string Name => nameof(SuperAdminSeeder);
    public override SeederType Type => SeederType.Infrastructure;
    public override int Order => 20;
    public override IReadOnlyList<string> DependsOn => [nameof(AppRoleSeeder)];

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();
        var userManager = context.UserManager;
        var adminData = context.Configuration.GetRequiredSection("SuperAdmin").Get<UserData>()!;
        var superAdmin = await userManager.FindByEmailAsync(adminData.Email);

        if (superAdmin is null)
        {
            superAdmin = new User
            {
                UserName = adminData.Email,
                FirstName = adminData.FirstName,
                LastName = adminData.LastName,
                Email = adminData.Email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(superAdmin, adminData.Password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create super admin: {result.Errors.First().Description}");
            }

            logger.LogInformation("Created super admin '{Admin}'", superAdmin.UserName);
        }
        else
        {
            // Upsert: update name if changed
            var updated = false;
            if (superAdmin.FirstName != adminData.FirstName)
            {
                superAdmin.FirstName = adminData.FirstName;
                updated = true;
            }

            if (superAdmin.LastName != adminData.LastName)
            {
                superAdmin.LastName = adminData.LastName;
                updated = true;
            }

            if (updated)
            {
                await userManager.UpdateAsync(superAdmin);
                logger.LogInformation("Updated super admin '{Admin}'", superAdmin.UserName);
            }
        }

        var hasSuperAdminRole = await userManager.IsInRoleAsync(superAdmin, AppRoles.SuperAdmin);
        if (!hasSuperAdminRole)
        {
            await userManager.AddToRoleAsync(superAdmin, AppRoles.SuperAdmin);
            logger.LogInformation("Added '{Role}' role to user '{Admin}'", AppRoles.SuperAdmin, superAdmin.UserName);
        }

        LogCompleted();
    }
}
