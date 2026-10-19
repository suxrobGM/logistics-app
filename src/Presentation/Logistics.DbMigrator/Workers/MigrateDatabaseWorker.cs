using Open.IdentityServer.EntityFramework.DbContexts;
using Logistics.Domain.Entities;
using Logistics.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Logistics.DbMigrator.Workers;

public class MigrateDatabaseWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<MigrateDatabaseWorker> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var masterDb = scope.ServiceProvider.GetRequiredService<MasterDbContext>();

        logger.LogInformation("Applying migrations to Master database...");
        await masterDb.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Master database migrated successfully");

        var grantDb = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
        logger.LogInformation("Applying migrations to the identity operational store (master DB)...");
        await grantDb.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Identity operational store migrated successfully");

        var tenants = await masterDb.Set<Tenant>().ToListAsync(cancellationToken);

        logger.LogInformation("Found {Count} tenant database(s) to migrate", tenants.Count);

        // Migrate each tenant database individually
        var failedTenants = new List<string>();
        foreach (var tenant in tenants)
        {
            try
            {
                using var tenantScope = scopeFactory.CreateScope();
                var tenantDb = tenantScope.ServiceProvider.GetRequiredService<TenantDbContext>();
                tenantDb.SwitchToTenant(tenant);

                logger.LogInformation("Applying migrations to tenant '{TenantName}'...", tenant.Name);
                await tenantDb.Database.MigrateAsync(cancellationToken);
                logger.LogInformation("Tenant '{TenantName}' migrated successfully", tenant.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to migrate tenant '{TenantName}'", tenant.Name);
                failedTenants.Add(tenant.Name);
            }
        }

        if (failedTenants.Count > 0)
        {
            logger.LogWarning("Migration failed for {Count} tenant(s): {Tenants}",
                failedTenants.Count, string.Join(", ", failedTenants));
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
