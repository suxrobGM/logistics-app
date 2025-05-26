using Logistics.Infrastructure.EF.Data;
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
        var tenantDb = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        logger.LogInformation("Applying migrations to Master database...");
        await masterDb.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Migration applied to Master database");
        
        logger.LogInformation("Applying migrations to Tenant database...");
        await tenantDb.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Migration applied to Tenant database");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
