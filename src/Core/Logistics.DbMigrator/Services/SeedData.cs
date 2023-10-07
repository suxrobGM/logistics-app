using System.Security.Claims;
using Logistics.Domain.Entities;
using Logistics.Domain.Services;
using Logistics.Domain.ValueObjects;
using Logistics.Infrastructure.EF.Data;
using Logistics.Shared.Policies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CustomClaimTypes = Logistics.Shared.Claims.CustomClaimTypes;

namespace Logistics.DbMigrator.Services;

internal class SeedData : BackgroundService
{
    private readonly ILogger<SeedData> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SeedData(
        ILogger<SeedData> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var mainDbContext = scope.ServiceProvider.GetRequiredService<MainDbContext>();
            var tenantDbContext = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

            _logger.LogInformation("Initializing main database...");
            await MigrateDatabaseAsync(mainDbContext);
            _logger.LogInformation("Successfully initialized the main database");

            _logger.LogInformation("Initializing tenant database...");
            await MigrateDatabaseAsync(tenantDbContext);
            _logger.LogInformation("Successfully initialized the tenant database");
            
            _logger.LogInformation("Seeding data...");
            await AddAppRolesAsync(scope.ServiceProvider);
            await AddSuperAdminAsync(scope.ServiceProvider);
            await AddDefaultTenantAsync(scope.ServiceProvider);
            _logger.LogInformation("Successfully seeded databases");

            var populateTestData = new PopulateFakeData(_logger, scope.ServiceProvider);
            await populateTestData.ExecuteAsync();
            _logger.LogInformation("Finished all operations!");
        }
        catch (Exception ex)
        {
            _logger.LogError("Thrown exception in SeedData.ExecuteAsync(): {Exception}", ex);
        }
    }

    private async Task MigrateDatabaseAsync(DbContext databaseContext)
    {
        await databaseContext.Database.MigrateAsync();
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

            var roleExists = await roleManager.RoleExistsAsync(role.Name!);
            if (roleExists)
                continue;
            
            var result = await roleManager.CreateAsync(role);
            await AddPermissions(roleManager, role, AppRolePermissions.GetBasicPermissions());

            switch (role.Name)
            {
                case AppRoles.SuperAdmin:
                    await AddPermissions(roleManager, role, AppRolePermissions.SuperAdmin);
                    break;
                case AppRoles.Admin:
                    await AddPermissions(roleManager, role, AppRolePermissions.Admin);
                    break;
                case AppRoles.Manager:
                    await AddPermissions(roleManager, role, AppRolePermissions.Manager);
                    break;
            }

            if (result.Succeeded)
                _logger.LogInformation("Added the '{RoleName}' role", appRole.Value);
        }
    }

    private async Task AddSuperAdminAsync(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

        var adminData = configuration.GetSection("SuperAdmin").Get<UserDto>();

        if (adminData == null)
            return;
        
        var superAdmin = await userManager.FindByEmailAsync(adminData.Email!);
        
        if (superAdmin is null)
        {
            superAdmin = new User
            {
                UserName = adminData.Email,
                FirstName = adminData.FirstName ?? "Admin",
                LastName = adminData.LastName ?? "Admin",
                Email = adminData.Email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(superAdmin, adminData.Password!);
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

    private async Task AddDefaultTenantAsync(IServiceProvider serviceProvider)
    {
        var mainDbContext = serviceProvider.GetRequiredService<MainDbContext>();
        var databaseProvider = serviceProvider.GetRequiredService<ITenantDatabaseService>();

        var defaultTenant = new Tenant
        {
            Name = "default",
            DisplayName = "Test Company",
            ConnectionString = databaseProvider.GenerateConnectionString("default") 
        };

        var existingTenant = mainDbContext.Set<Tenant>().FirstOrDefault(i => i.Name == defaultTenant.Name);

        if (existingTenant is null)
        {
            mainDbContext.Add(defaultTenant);
            await mainDbContext.SaveChangesAsync();
            await databaseProvider.CreateDatabaseAsync(defaultTenant.ConnectionString);
            _logger.LogInformation("Added default tenant");
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
        
        if (!allClaims.Any(i => i.Type == CustomClaimTypes.Permission && i.Value == permission))
        {
            var result = await roleManager.AddClaimAsync(role, claim);

            if (result.Succeeded)
                _logger.LogInformation("Added claim '{ClaimType}' - '{ClaimValue}' to the role '{Role}'", claim.Type, claim.Value, role.Name);
        }
    }
}
