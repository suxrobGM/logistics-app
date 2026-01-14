using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.DbMigrator.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Microsoft.AspNetCore.Identity;

namespace Logistics.DbMigrator.Workers;

/// <summary>
/// Orchestrates all seeders in the correct order.
/// Replaces both SeedDatabaseWorker and FakeDataWorker.
/// </summary>
internal class SeederOrchestrationWorker(
    ILogger<SeederOrchestrationWorker> logger,
    IServiceScopeFactory scopeFactory)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var populateFakeData = configuration.GetValue<bool>("PopulateFakeData");

        var context = new SeederContext
        {
            ServiceProvider = scope.ServiceProvider,
            Configuration = configuration,
            MasterUnitOfWork = scope.ServiceProvider.GetRequiredService<IMasterUnitOfWork>(),
            TenantUnitOfWork = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>(),
            UserManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>(),
            RoleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>()
        };

        // Resolve seeders from scope and order them
        var allSeeders = scope.ServiceProvider.GetServices<ISeeder>();
        var seeders = SeederRegistry.GetOrderedSeeders(allSeeders, logger);
        logger.LogInformation("Starting database seeding with {Count} seeders", seeders.Count);

        foreach (var seeder in seeders)
        {
            if (seeder.Type == SeederType.FakeData && !populateFakeData)
            {
                logger.LogInformation("Skipping {SeederName} (PopulateFakeData is disabled)", seeder.Name);
                continue;
            }

            if (await seeder.ShouldSkipAsync(context, cancellationToken))
            {
                logger.LogInformation("Skipping {SeederName} (data already exists)", seeder.Name);
                continue;
            }

            try
            {
                await seeder.SeedAsync(context, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in {SeederName}", seeder.Name);
                throw;
            }
        }

        logger.LogInformation("Database seeding completed successfully");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
