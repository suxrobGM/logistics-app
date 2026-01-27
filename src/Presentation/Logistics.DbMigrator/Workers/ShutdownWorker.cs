namespace Logistics.DbMigrator.Workers;

/// <summary>
///     Shuts down the application after all migration and seeding workers complete.
///     This allows other services in Aspire to wait for DbMigrator completion using WaitForCompletion().
/// </summary>
internal class ShutdownWorker(
    ILogger<ShutdownWorker> logger,
    IHostApplicationLifetime lifetime)
    : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("All migration and seeding tasks completed. Shutting down DbMigrator...");

        // Request graceful shutdown
        lifetime.StopApplication();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
