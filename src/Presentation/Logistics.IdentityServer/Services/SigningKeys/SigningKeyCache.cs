using Logistics.Infrastructure.Persistence.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using Open.IdentityServer.Models;

namespace Logistics.IdentityServer.Services.SigningKeys;

/// <summary>
///     Holds the materialized signing key set. Reads through its own scope, never the request's
///     <see cref="MasterDbContext"/>, so a read here cannot flush someone else's tracked changes.
/// </summary>
public class SigningKeyCache(
    IServiceScopeFactory scopeFactory,
    IOptions<SigningKeyOptions> options,
    ILogger<SigningKeyCache> logger)
{
    private readonly SemaphoreSlim refreshLock = new(1, 1);
    private readonly SigningKeyOptions options = options.Value;

    private SigningCredentials? active;
    private DateTimeOffset expiresAt = DateTimeOffset.MinValue;
    private IReadOnlyList<SecurityKeyInfo> validation = [];

    public void Invalidate() => expiresAt = DateTimeOffset.MinValue;

    public async Task<SigningCredentials?> GetActiveAsync()
    {
        await RefreshIfStaleAsync();
        return active;
    }

    public async Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
    {
        await RefreshIfStaleAsync();
        return validation;
    }

    private async Task RefreshIfStaleAsync()
    {
        if (DateTimeOffset.UtcNow < expiresAt)
        {
            return;
        }

        await refreshLock.WaitAsync();
        try
        {
            if (DateTimeOffset.UtcNow < expiresAt)
            {
                return;
            }

            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
            var protector = scope.ServiceProvider.GetRequiredService<SigningKeyProtector>();

            var keys = await db.SigningKeys
                .AsNoTracking()
                .OrderByDescending(x => x.Created)
                .ToListAsync();

            var credentials = keys.Select(protector.Unprotect).ToList();

            // Sign with the newest key that has been published long enough for relying parties to
            // have picked it up. If none has, fall back to the oldest available, which is the one
            // they are most likely to already hold.
            var cutoff = DateTime.UtcNow - options.Propagation;
            var activeIndex = keys.FindIndex(x => x.Created <= cutoff);
            if (activeIndex < 0)
            {
                activeIndex = credentials.Count - 1;
            }

            active = credentials.Count == 0 ? null : credentials[activeIndex];
            validation = credentials
                .Select(x => new SecurityKeyInfo { Key = x.Key, SigningAlgorithm = x.Algorithm })
                .ToList();
            expiresAt = DateTimeOffset.UtcNow.Add(options.CacheTtl);

            logger.LogDebug("Loaded {Count} signing key(s), active kid {Kid}",
                credentials.Count, active?.Key.KeyId ?? "none");
        }
        finally
        {
            refreshLock.Release();
        }
    }
}
