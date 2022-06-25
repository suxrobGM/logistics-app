using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Logistics.Domain.Entities;
using Logistics.Domain.Services;
using Logistics.Domain.ValueObjects;
using Logistics.EntityFramework.Data;

namespace Logistics.DbMigrator
{
    public class SeedDataService : BackgroundService
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
            _logger.LogInformation("Successfully seeded databases");
        }

        private async Task MigrateDatabaseAsync(DbContext databaseContext)
        {
            try
            {
                await databaseContext.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Thrown exception in SeedData.MigrateDatabaseAsync(): {Exception}", ex);
            }
        }

        private async Task AddDefaultAdminAsync(IServiceProvider serviceProvider)
        {
            try
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

                var adminUserName = configuration["DefaultAdmin:UserName"];
                var adminEmail = configuration["DefaultAdmin:Email"];
                var adminPassword = configuration["DefaultAdmin:Password"];
                var admin = await userManager.FindByEmailAsync(adminEmail);

                if (admin is null)
                {
                    admin = new User
                    {
                        UserName = adminUserName,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        Role = UserRole.Admin
                    };

                    var result = await userManager.CreateAsync(admin, adminPassword);
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
            catch (Exception ex)
            {
                _logger.LogError("Thrown exception in SeedDataService.AddDefaultAdminAsync(): {Exception}", ex);
            }
        }

        private async Task AddDefaultTenantAsync(IServiceProvider serviceProvider)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError("Thrown exception in SeedDataService.AddDefaultTenantAsync(): {Exception}", ex);
            }
        }
    }
}