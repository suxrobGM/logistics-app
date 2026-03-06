using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;
using Logistics.Shared.Identity.Roles;
using Microsoft.AspNetCore.Identity;

namespace Logistics.DbMigrator.Seeders.Infrastructure;

/// <summary>
///     Seeds and syncs the super admin user from configuration.
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

        // Find by role first (survives email changes), then fall back to email
        var superAdmins = await userManager.GetUsersInRoleAsync(AppRoles.SuperAdmin);
        var superAdmin = superAdmins.FirstOrDefault()
                         ?? await userManager.FindByEmailAsync(adminData.Email);

        if (superAdmin is null)
        {
            superAdmin = await CreateSuperAdminAsync(userManager, adminData);
        }
        else
        {
            await SyncSuperAdminAsync(userManager, superAdmin, adminData);
        }

        if (!await userManager.IsInRoleAsync(superAdmin, AppRoles.SuperAdmin))
        {
            await userManager.AddToRoleAsync(superAdmin, AppRoles.SuperAdmin);
            logger.LogInformation("Assigned '{Role}' role to '{Admin}'", AppRoles.SuperAdmin, superAdmin.Email);
        }

        LogCompleted();
    }

    private async Task<User> CreateSuperAdminAsync(UserManager<User> userManager, UserData adminData)
    {
        var user = new User
        {
            UserName = adminData.Email,
            Email = adminData.Email,
            FirstName = adminData.FirstName,
            LastName = adminData.LastName,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, adminData.Password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create super admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        logger.LogInformation("Created super admin '{Admin}'", user.Email);
        return user;
    }

    private async Task SyncSuperAdminAsync(UserManager<User> userManager, User superAdmin, UserData adminData)
    {
        var updated = false;

        if (!string.Equals(superAdmin.Email, adminData.Email, StringComparison.OrdinalIgnoreCase))
        {
            await RemoveDuplicateUserAsync(userManager, superAdmin.Id, adminData.Email);
            superAdmin.Email = adminData.Email;
            superAdmin.UserName = adminData.Email;
            superAdmin.NormalizedEmail = adminData.Email.ToUpperInvariant();
            superAdmin.NormalizedUserName = adminData.Email.ToUpperInvariant();
            updated = true;
        }

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

        var hasher = userManager.PasswordHasher;

        if (hasher.VerifyHashedPassword(superAdmin, superAdmin.PasswordHash!, adminData.Password)
            == PasswordVerificationResult.Failed)
        {
            superAdmin.PasswordHash = hasher.HashPassword(superAdmin, adminData.Password);
            updated = true;
        }

        if (!updated)
        {
            logger.LogInformation("Super admin already up to date");
            return;
        }

        var result = await userManager.UpdateAsync(superAdmin);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to sync super admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        logger.LogInformation("Synced super admin '{Admin}'", superAdmin.Email);
    }

    private async Task RemoveDuplicateUserAsync(UserManager<User> userManager, Guid superAdminId, string targetEmail)
    {
        var duplicate = await userManager.FindByEmailAsync(targetEmail);
        if (duplicate is not null && duplicate.Id != superAdminId)
        {
            await userManager.DeleteAsync(duplicate);
            logger.LogInformation("Removed duplicate user '{Email}' blocking super admin email change", targetEmail);
        }
    }
}
