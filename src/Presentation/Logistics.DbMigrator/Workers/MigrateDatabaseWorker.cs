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

        // Query all tenants with valid connection strings from the master database
        var tenants = await masterDb.Set<Tenant>()
            .Where(t => t.ConnectionString != null && t.ConnectionString != "")
            .Select(t => new { t.Name, t.ConnectionString })
            .ToListAsync(cancellationToken);

        logger.LogInformation("Found {Count} tenant database(s) to migrate", tenants.Count);

        // Migrate each tenant database individually
        var failedTenants = new List<string>();
        foreach (var tenant in tenants)
        {
            try
            {
                using var tenantScope = scopeFactory.CreateScope();
                var tenantDb = tenantScope.ServiceProvider.GetRequiredService<TenantDbContext>();
                tenantDb.Database.SetConnectionString(tenant.ConnectionString);

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
