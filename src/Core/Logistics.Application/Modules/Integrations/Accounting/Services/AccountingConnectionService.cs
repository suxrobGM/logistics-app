using System.Collections.Concurrent;
using Logistics.Application.Abstractions.Accounting;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.Accounting.Services;

internal sealed class AccountingConnectionService(
    ITenantUnitOfWork tenantUow,
    IAccountingProviderFactory providerFactory,
    ILogger<AccountingConnectionService> logger) : IAccountingConnectionService
{
    private static readonly TimeSpan RefreshLeeway = TimeSpan.FromMinutes(5);

    // The recurring job and a manual "Sync now" can overlap; one refresh at a time per connection.
    // Multi-instance would need a distributed lock.
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> RefreshLocks = new();

    public async Task<IAccountingProviderService?> GetProviderAsync(
        AccountingProviderConfiguration config, CancellationToken ct)
    {
        if (config.NeedsReconnect || string.IsNullOrEmpty(config.RefreshToken))
        {
            return null;
        }

        if (!HasValidToken(config))
        {
            var refreshLock = RefreshLocks.GetOrAdd(config.Id, _ => new SemaphoreSlim(1, 1));
            await refreshLock.WaitAsync(ct);
            try
            {
                if (!HasValidToken(config) && !await RefreshAsync(config, ct))
                {
                    return null;
                }
            }
            finally
            {
                refreshLock.Release();
            }
        }

        return providerFactory.GetProvider(config);
    }

    public async Task RejectAccessTokenAsync(AccountingProviderConfiguration config, CancellationToken ct)
    {
        logger.LogWarning("{Provider} rejected the access token for tenant {TenantId}; refreshing on the next run",
            config.ProviderType, tenantUow.GetCurrentTenant().Id);
        config.TokenExpiresAt = null;
        await tenantUow.SaveChangesAsync(ct);
    }

    private static bool HasValidToken(AccountingProviderConfiguration config) =>
        !string.IsNullOrEmpty(config.AccessToken)
        && config.TokenExpiresAt is { } expiresAt
        && expiresAt - DateTime.UtcNow > RefreshLeeway;

    private async Task<bool> RefreshAsync(AccountingProviderConfiguration config, CancellationToken ct)
    {
        try
        {
            var provider = providerFactory.GetProvider(config.ProviderType);
            var token = await provider.RefreshTokenAsync(config.RefreshToken!, ct);

            config.AccessToken = token.AccessToken;
            config.RefreshToken = token.RefreshToken ?? config.RefreshToken;
            config.TokenExpiresAt = token.ExpiresAt;
            config.RefreshTokenExpiresAt = token.RefreshTokenExpiresAt ?? config.RefreshTokenExpiresAt;
        }
        catch (AccountingReconnectRequiredException ex)
        {
            logger.LogWarning(ex, "{Provider} refused the refresh token for tenant {TenantId}; the user must reconnect",
                config.ProviderType, tenantUow.GetCurrentTenant().Id);
            config.NeedsReconnect = true;
            config.AccessToken = null;
        }

        await tenantUow.SaveChangesAsync(ct);
        return !config.NeedsReconnect;
    }
}
