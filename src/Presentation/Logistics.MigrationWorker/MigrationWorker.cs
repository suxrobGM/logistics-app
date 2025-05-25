using Logistics.Infrastructure.EF.Data;
using Microsoft.EntityFrameworkCore;

namespace Logistics.MigrationWorker;

public class MigrationWorker(IServiceProvider services, ILogger<MigrationWorker> logger) : BackgroundService
{
    private readonly IServiceProvider _services = services;
    private readonly ILogger<MigrationWorker> _logger = logger;


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _services.CreateScope();

        var masterDb = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
        var tenantDb = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        _logger.LogInformation("Applying migrations...");

        await masterDb.Database.MigrateAsync(stoppingToken);
        await tenantDb.Database.MigrateAsync(stoppingToken);

        _logger.LogInformation("Migration applied");
    }
}