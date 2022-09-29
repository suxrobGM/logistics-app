using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Logistics.Domain.Shared;
using ClaimTypes = Logistics.Domain.Shared.ClaimTypes;

namespace Logistics.DbMigrator.Services;

internal class SeedDataService : BackgroundService
{
    private readonly ILogger<SeedDataService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SeedDataService(
        ILogger<SeedDataService> logger,
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

            _logger.LogInformation("Initializing main database");
            await MigrateDatabaseAsync(mainDbContext);
            _logger.LogInformation("Successfully initialized the main database");

            _logger.LogInformation("Initializing tenant database");
            await MigrateDatabaseAsync(tenantDbContext);
            _logger.LogInformation("Successfully initialized the tenant database");

            await AddAppRolesAsync(scope.ServiceProvider);
            await AddSuperAdminAsync(scope.ServiceProvider);
            await AddDefaultTenantAsync(scope.ServiceProvider);
            _logger.LogInformation("Successfully seeded databases");

            var populateTestData = new PopulateTestData(_logger, scope.ServiceProvider);
            await populateTestData.ExecuteAsync();
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

            var roleExists = await roleManager.RoleExistsAsync(role.Name);
            if (roleExists)
                continue;
            
            var result = await roleManager.CreateAsync(role);

            await AddBasicPermissions(roleManager, role);

            switch (role.Name)
            {
                case AppRoles.SuperAdmin:
                    await AddAllPermissions(roleManager, role);
                    break;
                case AppRoles.Admin:
                    await AddPermissions(roleManager, role, "AppRole");
                    await AddPermissions(roleManager, role, "Employee");
                    await AddPermissions(roleManager, role, "Load");
                    await AddPermissions(roleManager, role, "Truck");
                    await AddPermissions(roleManager, role, "TenantRole");
                    await AddPermission(roleManager, role, Permissions.Report.View);
                    await AddPermission(roleManager, role, Permissions.Tenant.Create);
                    await AddPermission(roleManager, role, Permissions.Tenant.Edit);
                    await AddPermission(roleManager, role, Permissions.Tenant.View);
                    break;
                case AppRoles.Manager:
                    await AddPermission(roleManager, role, Permissions.Tenant.View);
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

        var userData = configuration.GetSection("SuperAdmin").Get<UserDto>();
        var superAdmin = await userManager.FindByEmailAsync(userData.Email);
        
        if (superAdmin is null)
        {
            superAdmin = new User
            {
                UserName = userData.UserName,
                Email = userData.Email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(superAdmin, userData.Password);
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
        var databaseProvider = serviceProvider.GetRequiredService<IDatabaseProviderService>();

        var defaultTenant = new Tenant
        {
            Name = "default",
            DisplayName = "Default Tenant",
            ConnectionString = databaseProvider.GenerateConnectionString("u1002275_default_logistics") // TODO: remove prefix u1002275_ later 
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
    
    private async Task AddAllPermissions(RoleManager<AppRole> roleManager, AppRole role)
    {
        var permissions = Permissions.GetAll();
        foreach (var permission in permissions)
        {
            await AddPermission(roleManager, role, permission);
        }
    }
    
    private async Task AddPermissions(RoleManager<AppRole> roleManager, AppRole role, string module)
    {
        var permissions = Permissions.GeneratePermissions(module);
        foreach (var permission in permissions)
        {
            await AddPermission(roleManager, role, permission);
        }
    }
    
    private async Task AddPermission(RoleManager<AppRole> roleManager, AppRole role, string permission)
    {
        var allClaims = await roleManager.GetClaimsAsync(role);
        var claim = new Claim(ClaimTypes.Permission, permission);
        
        if (!allClaims.Any(i => i.Type == ClaimTypes.Permission && i.Value == permission))
        {
            var result = await roleManager.AddClaimAsync(role, claim);

            if (result.Succeeded)
                _logger.LogInformation("Added claim '{ClaimType}' - '{ClaimValue}' to the role '{Role}'", claim.Value, claim.Type, role.Name);
        }
    }

    private async Task AddBasicPermissions(RoleManager<AppRole> roleManager, AppRole role)
    {
        await AddPermission(roleManager, role, Permissions.AppRole.View);
        await AddPermission(roleManager, role, Permissions.TenantRole.View);
        await AddPermission(roleManager, role, Permissions.User.View);
        await AddPermission(roleManager, role, Permissions.Employee.View);
    }
}