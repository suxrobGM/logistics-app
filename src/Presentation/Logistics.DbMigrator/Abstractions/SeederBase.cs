using Logistics.DbMigrator.Models;

namespace Logistics.DbMigrator.Abstractions;

/// <summary>
/// Abstract base class providing common seeder functionality.
/// </summary>
public abstract class SeederBase(ILogger logger) : ISeeder
{
    protected readonly ILogger logger = logger;
    protected readonly Random random = new();

    public abstract string Name { get; }
    public abstract SeederType Type { get; }
    public virtual int Order => 100;
    public virtual IReadOnlyList<string> DependsOn => [];

    public abstract Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Default implementation for ShouldSkipAsync.
    /// Infrastructure seeders never skip; FakeData seeders skip if data exists.
    /// </summary>
    public virtual async Task<bool> ShouldSkipAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        if (Type == SeederType.Infrastructure)
        {
            return false;
        }

        return await HasExistingDataAsync(context, cancellationToken);
    }

    /// <summary>
    /// Override in FakeData seeders to check if data already exists.
    /// </summary>
    protected virtual Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult(false);
    }

    protected void LogSkipping(string reason)
    {
        logger.LogInformation("Skipping {SeederName}: {Reason}", Name, reason);
    }

    protected void LogStarting()
    {
        logger.LogInformation("Starting {SeederName}...", Name);
    }

    protected void LogCompleted(int count = 0)
    {
        if (count > 0)
        {
            logger.LogInformation("Completed {SeederName}: {Count} entities processed", Name, count);
        }
        else
        {
            logger.LogInformation("Completed {SeederName}", Name);
        }
    }
}
