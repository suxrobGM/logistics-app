using Logistics.Infrastructure.Persistence.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Open.IdentityServer.EntityFramework.DbContexts;

namespace Logistics.IdentityServer.Services.SigningKeys;

/// <summary>
///     Keeps the signing key set healthy: publishes a replacement before the current key expires,
///     deletes keys nobody can still hold a token for, and deletes used refresh grants.
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

    private readonly SigningKeyOptions _options = options.Value;

    public async Task EnsureKeysAsync(CancellationToken cancellationToken)
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

            // Publish the replacement a propagation window before the current key expires.
            if (keys.Count == 0 || keys[0].Created <= now - (_options.Rotation - _options.Propagation))
            {
                db.SigningKeys.Add(protector.Create());
                rotated = true;
            }

            // Keep the two newest regardless, so deleting can never leave us unable to sign.
            var expired = keys.Skip(2).Where(x => x.Created <= now - _options.Retention).ToList();
            if (expired.Count > 0)
            {
                db.SigningKeys.RemoveRange(expired);
                logger.LogInformation("Deleted {Count} expired signing key(s)", expired.Count);
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
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Period);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await EnsureKeysAsync(stoppingToken);
                await DeleteUsedGrantsAsync(stoppingToken);
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
    private async Task DeleteUsedGrantsAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var grants = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
        var cutoff = DateTime.UtcNow - _options.UsedGrantRetention;

        var removed = await grants.Database.CreateExecutionStrategy().ExecuteAsync(() =>
            grants.PersistedGrants
                .Where(x => x.ConsumedTime != null && x.ConsumedTime < cutoff)
                .ExecuteDeleteAsync(cancellationToken));

        if (removed > 0)
        {
            logger.LogInformation("Deleted {Count} used grant(s)", removed);
        }
    }
}
