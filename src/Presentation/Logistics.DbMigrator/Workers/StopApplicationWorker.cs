namespace Logistics.DbMigrator.Workers;

/// <summary>
/// Registered last, only with <c>--exit</c>: hosted services start in order, so migration and
/// seeding are done by the time this stops the host.
/// </summary>
internal class StopApplicationWorker(
    ILogger<StopApplicationWorker> logger,
    IHostApplicationLifetime lifetime) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Deferred: stopping mid-startup makes Host.Run exit non-zero.
        lifetime.ApplicationStarted.Register(() =>
        {
            logger.LogInformation("All migration and seeding work completed - exiting (--exit)");
            lifetime.StopApplication();
        });
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
