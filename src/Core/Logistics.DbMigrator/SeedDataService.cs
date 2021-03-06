using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Logistics.Domain.Entities;
using Logistics.Domain.Services;
using Logistics.Domain.ValueObjects;
using Logistics.EntityFramework.Data;

namespace Logistics.DbMigrator;

public class SeedDataService : BackgroundService
{
    private readonly IHostEnvironment _env;
    private readonly ILogger<SeedDataService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SeedDataService(
        IHostEnvironment env,
        ILogger<SeedDataService> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _env = env;
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

            await AddDefaultAdminAsync(scope.ServiceProvider);
            await AddDefaultTenantAsync(scope.ServiceProvider);

            if (_env.IsDevelopment())
                await AddTestUsersAsync(scope.ServiceProvider);
        
            _logger.LogInformation("Successfully seeded databases");
        }
        catch (Exception ex)
        {
            _logger.LogError("Thrown exception in SeedDataService.ExecuteAsync(): {Exception}", ex);
        }
    }

    private async Task MigrateDatabaseAsync(DbContext databaseContext)
    {
        await databaseContext.Database.MigrateAsync();
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
                EmailConfirmed = true,
                Role = UserRole.Admin
            };

            var result = await userManager.CreateAsync(admin, adminDto.Password);
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            _logger.LogInformation("Created the default admin");
        }
        else if (admin.Role != UserRole.Admin)
        {
            admin.Role = UserRole.Admin;
            await userManager.UpdateAsync(admin);
            _logger.LogInformation("Added 'admin' role to the default admin");
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

    private async Task AddTestUsersAsync(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var testUsers = configuration.GetSection("TestUsers").Get<UserDto[]>();

        foreach (var testUser in testUsers)
        {
            var user = await userManager.FindByNameAsync(testUser.UserName);

            if (user != null)
                continue;
            
            user = new User
            {
                UserName = testUser.UserName,
                Email = testUser.Email,
                EmailConfirmed = true,
                Role = UserRole.Guest,
            };

            try
            {
                var result = await userManager.CreateAsync(user, testUser.Password);
                if (!result.Succeeded)
                    throw new Exception(result.Errors.First().Description);
            }
            finally
            {
                _logger.LogInformation("Created the test user {UserName}", testUser.UserName);
            }
        }
    }
}

internal record UserDto
{
    public string? UserName { get; init; }
    public string? Email { get; init; }
    public string? Password { get; init; }
}