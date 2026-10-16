using System.Collections.Concurrent;
using Logistics.Application.Abstractions.Accounting;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
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

    public async Task<Result<T>> ExecuteAsync<T>(
        Func<IAccountingProviderService, AccountingProviderConfiguration, Task<T>> call, CancellationToken ct)
    {
        var config = await tenantUow.Repository<AccountingProviderConfiguration>()
            .GetAsync(c => c.ProviderType == AccountingProviderType.QuickBooksOnline && c.IsActive, ct);
        if (config is null)
        {
            return Result<T>.Fail("QuickBooks is not connected.");
        }

        var provider = await GetProviderAsync(config, ct);
        if (provider is null)
        {
            return Result<T>.Fail("QuickBooks needs to be reconnected.");
        }

        try
        {
            return Result<T>.Ok(await call(provider, config));
        }
        catch (AccountingReconnectRequiredException ex)
        {
            await RejectAccessTokenAsync(config, ex, ct);
            return Result<T>.Fail("QuickBooks rejected the connection. Try again in a moment, or reconnect.");
        }
    }

    public async Task<Result> ExecuteAsync(
        Func<IAccountingProviderService, AccountingProviderConfiguration, Task<Result>> call, CancellationToken ct)
    {
        var result = await ExecuteAsync<Result>(call, ct);
        return result.IsSuccess ? result.Value! : Result.Fail(result.Error!);
    }

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

    private async Task RejectAccessTokenAsync(
        AccountingProviderConfiguration config, AccountingReconnectRequiredException ex, CancellationToken ct)
    {
        logger.LogWarning(ex, "{Provider} rejected the access token for tenant {TenantId}; refreshing on the next run",
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
