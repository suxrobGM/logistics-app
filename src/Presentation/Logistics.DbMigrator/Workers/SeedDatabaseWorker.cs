using System.Security.Claims;

using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Domain.Services;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Identity.Roles;

using Microsoft.AspNetCore.Identity;

using CustomClaimTypes = Logistics.Shared.Identity.Claims.CustomClaimTypes;

namespace Logistics.DbMigrator.Workers;

internal class SeedDatabaseWorker : IHostedService
{
    private const string UserDefaultPassword = "Test12345#";
    private readonly ILogger<SeedDatabaseWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public SeedDatabaseWorker(
        ILogger<SeedDatabaseWorker> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        _logger.LogInformation("Seeding databases...");
        await AddAppRolesAsync(scope.ServiceProvider);
        await AddSuperAdminAsync(scope.ServiceProvider);
        //await AddSubscriptionPlanAsync(scope.ServiceProvider);
        await AddDefaultTenantAsync(scope.ServiceProvider);
        _logger.LogInformation("Successfully seeded databases");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task AddAppRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<AppRole>>();
        var appRoles = AppRoles.GetValues();

        foreach (var appRole in appRoles)
        {
            var role = new AppRole(appRole.Value)
            {
                DisplayName = appRole.DisplayName
            };

            var existingRole = await roleManager.FindByNameAsync(role.Name!);
            if (existingRole is not null)
            {
                // Update existing role claims
                await AddPermissions(roleManager, existingRole, GetPermissionsBasedOnRole(existingRole.Name!));
                _logger.LogInformation("Updated app role '{Role}'", existingRole.Name);
            }
            else
            {
                // Add a new role and its claims
                var result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    await AddPermissions(roleManager, role, AppRolePermissions.GetBasicPermissions());
                    await AddPermissions(roleManager, role, GetPermissionsBasedOnRole(role.Name!));
                    _logger.LogInformation("Added the '{RoleName}' role", role.Name);
                }
                else
                {
                    _logger.LogError("Failed to add the '{RoleName}' role", role.Name);
                }
            }
        }
    }

    private async Task AddSuperAdminAsync(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var adminData = configuration.GetRequiredSection("SuperAdmin").Get<User>()!;
        var superAdmin = await userManager.FindByEmailAsync(adminData.Email!);

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

            var result = await userManager.CreateAsync(superAdmin, UserDefaultPassword);
            if (!result.Succeeded)
                throw new Exception(result.Errors.First().Description);

            _logger.LogInformation("Created the super admin '{Admin}'", superAdmin.UserName);
        }

        var hasSuperAdminRole = await userManager.IsInRoleAsync(superAdmin, AppRoles.SuperAdmin);

        if (!hasSuperAdminRole)
        {
            await userManager.AddToRoleAsync(superAdmin, AppRoles.SuperAdmin);
            _logger.LogInformation("Added 'app.super_admin' role to the user '{Admin}'", superAdmin.UserName);
        }
    }

    private async Task AddSubscriptionPlanAsync(IServiceProvider serviceProvider)
    {
        var masterUow = serviceProvider.GetRequiredService<IMasterUnityOfWork>();
        var standardPlan = new SubscriptionPlan
        {
            Name = "Standard",
            Description = "Standard monthly subscription plan charging per employee",
            Price = 30
        };

        var existingPlan = await masterUow.Repository<SubscriptionPlan>().GetAsync(i => i.Name == standardPlan.Name);

        if (existingPlan is null)
        {
            await masterUow.Repository<SubscriptionPlan>().AddAsync(standardPlan);
            await masterUow.SaveChangesAsync();
            _logger.LogInformation("Added a subscription plan {PlanName}", standardPlan.Name);
        }
    }

    private async Task AddDefaultTenantAsync(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var masterUow = serviceProvider.GetRequiredService<IMasterUnityOfWork>();
        var databaseProvider = serviceProvider.GetRequiredService<ITenantDatabaseService>();
        var defaultTenantConnectionString = configuration.GetConnectionString("DefaultTenantDatabase")
                                            ?? databaseProvider.GenerateConnectionString("default");
        var companyAddress = new Address
        {
            Line1 = "7 Allstate Rd",
            City = "Dorchester",
            State = "Massachusetts",
            ZipCode = "02125",
            Country = "United States"
        };

        var defaultTenant = new Tenant
        {
            Name = "default",
            CompanyName = "Test Company",
            BillingEmail = "test@gmail.com",
            CompanyAddress = companyAddress,
            ConnectionString = defaultTenantConnectionString
        };

        var existingTenant = await masterUow.Repository<Tenant>().GetAsync(i => i.Name == defaultTenant.Name);

        if (existingTenant is null)
        {
            await masterUow.Repository<Tenant>().AddAsync(defaultTenant);
            await masterUow.SaveChangesAsync();
            await databaseProvider.CreateDatabaseAsync(defaultTenant.ConnectionString);
            _logger.LogInformation("Added default tenant with connection string: {ConnectionString}", defaultTenant.ConnectionString);
        }
    }

    private async Task AddPermissions(RoleManager<AppRole> roleManager, AppRole role, IEnumerable<string> permissions)
    {
        foreach (var permission in permissions)
        {
            await AddPermission(roleManager, role, permission);
        }
    }

    private async Task AddPermission(RoleManager<AppRole> roleManager, AppRole role, string permission)
    {
        var allClaims = await roleManager.GetClaimsAsync(role);
        var claim = new Claim(CustomClaimTypes.Permission, permission);

        if (!allClaims.Any(i => i.Type == claim.Type && i.Value == claim.Value))
        {
            var result = await roleManager.AddClaimAsync(role, claim);

            if (result.Succeeded)
            {
                _logger.LogInformation("Added claim '{ClaimType}' - '{ClaimValue}' to the role '{Role}'", claim.Type,
                    claim.Value, role.Name);
            }
            else
            {
                _logger.LogError("Failed to add claim '{ClaimType}' - '{ClaimValue}' to the role '{Role}'", claim.Type,
                    claim.Value, role.Name);
            }
        }
    }

    private static IEnumerable<string> GetPermissionsBasedOnRole(string roleName)
    {
        // This method returns the specific permissions based on the role name.
        return roleName switch
        {
            AppRoles.SuperAdmin => AppRolePermissions.SuperAdmin,
            AppRoles.Admin => AppRolePermissions.Admin,
            AppRoles.Manager => AppRolePermissions.Manager,
            _ => []
        };
    }
}
