using Logistics.Infrastructure.Persistence.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Open.IdentityServer.EntityFramework.DbContexts;

namespace Logistics.IdentityServer.Services.SigningKeys;

/// <summary>
///     Keeps the signing key set healthy: publishes a successor before the current key ages out,
///     prunes keys nobody can still hold a token for, and sweeps consumed refresh grants.
///     Owns every write to <c>signing_keys</c>.
/// </summary>
public class SigningKeyMaintenance(
    IServiceScopeFactory scopeFactory,
    SigningKeyCache cache,
    IOptions<SigningKeyOptions> options,
    ILogger<SigningKeyMaintenance> logger) : BackgroundService
{
    /// <summary>Arbitrary but fixed: only this maintenance run may hold it.</summary>
    private const long AdvisoryLockId = 8_472_015_339_001;

    private static readonly TimeSpan Period = TimeSpan.FromHours(12);

    private readonly SigningKeyOptions options = options.Value;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
        var protector = scope.ServiceProvider.GetRequiredService<SigningKeyProtector>();

        var created = await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

            // Two instances starting together would otherwise both decide to create a key.
            await db.Database.ExecuteSqlAsync(
                $"SELECT pg_advisory_xact_lock({AdvisoryLockId})", cancellationToken);

            var keys = await db.SigningKeys.OrderByDescending(x => x.Created).ToListAsync(cancellationToken);
            var now = DateTime.UtcNow;
            var rotated = false;

            // Publish the successor a propagation window before the incumbent is due to retire.
            if (keys.Count == 0 || keys[0].Created <= now - (options.Rotation - options.Propagation))
            {
                db.SigningKeys.Add(protector.Create());
                rotated = true;
            }

            // Keep the two newest regardless, so pruning can never leave us unable to sign.
            var stale = keys.Skip(2).Where(x => x.Created <= now - options.Retention).ToList();
            if (stale.Count > 0)
            {
                db.SigningKeys.RemoveRange(stale);
                logger.LogInformation("Pruned {Count} retired signing key(s)", stale.Count);
            }

            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return rotated;
        });

        if (created)
        {
            cache.Invalidate();
            logger.LogInformation("Published a new signing key");
        }

        await SweepConsumedGrantsAsync(scope, cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Period);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await RunAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Signing key maintenance failed, retrying on the next tick");
            }
        }
    }

    /// <summary>
    ///     The operational store only expires grants, so one-time refresh tokens that were already
    ///     redeemed would otherwise sit there until their absolute lifetime runs out.
    /// </summary>
    private async Task SweepConsumedGrantsAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        var grants = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
        var cutoff = DateTime.UtcNow - options.ConsumedGrantRetention;

        var removed = await grants.Database.CreateExecutionStrategy().ExecuteAsync(() =>
            grants.PersistedGrants
                .Where(x => x.ConsumedTime != null && x.ConsumedTime < cutoff)
                .ExecuteDeleteAsync(cancellationToken));

        if (removed > 0)
        {
            logger.LogInformation("Swept {Count} consumed grant(s)", removed);
        }
    }
}
