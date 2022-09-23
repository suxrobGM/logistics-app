using Microsoft.AspNetCore.Identity;

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
            await AddDefaultAdminAsync(scope.ServiceProvider);
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

            if (result.Succeeded)
                _logger.LogInformation("Added the '{RoleName}' role", appRole.Value);
        }
    }

    private async Task AddDefaultAdminAsync(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

        var adminDto = configuration.GetSection("DefaultAdmin").Get<UserDto>();
        var admin = await userManager.FindByEmailAsync(adminDto.Email);
        
        if (admin is null)
        {
            admin = new User
            {
                UserName = adminDto.UserName,
                Email = adminDto.Email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, adminDto.Password);
            if (!result.Succeeded)
                throw new Exception(result.Errors.First().Description);

            _logger.LogInformation("Created the default admin '{Admin}'", admin.UserName);
        }

        var hasAdminRole = await userManager.IsInRoleAsync(admin, AppRoles.Admin);
        
        if (!hasAdminRole)
        {
            await userManager.AddToRoleAsync(admin, AppRoles.Admin);
            _logger.LogInformation("Added 'app.admin' role to the default admin '{Admin}'", admin.UserName);
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
}