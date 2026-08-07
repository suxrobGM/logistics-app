using System.Collections.Concurrent;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.LoadBoard;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Services;

internal sealed class LoadBoardTokenService(
    ITenantUnitOfWork tenantUow,
    ILoadBoardProviderFactory providerFactory,
    ILogger<LoadBoardTokenService> logger) : ILoadBoardTokenService
{
    /// <summary>Tokens are considered expired this long before their actual expiry.</summary>
    private static readonly TimeSpan ExpirySkew = TimeSpan.FromMinutes(2);

    // Serializes refreshes per configuration on this instance so parallel searches don't
    // burn the same refresh token twice. Multi-instance would need a distributed lock instead.
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> RefreshLocks = new();

    public async Task<Result<ILoadBoardProviderService>> GetReadyProviderAsync(
        LoadBoardConfiguration configuration, CancellationToken ct = default)
    {
        var provider = providerFactory.GetProvider(configuration.ProviderType);

        if (provider.RequiresOAuthToken && !HasValidToken(configuration))
        {
            var refreshLock = RefreshLocks.GetOrAdd(configuration.Id, _ => new SemaphoreSlim(1, 1));
            await refreshLock.WaitAsync(ct);
            try
            {
                if (!HasValidToken(configuration))
                {
                    var token = await AcquireOrRefreshAsync(provider, configuration);
                    if (token is null)
                    {
                        return Result<ILoadBoardProviderService>.Fail(
                            $"Could not authenticate with {configuration.ProviderType}. " +
                            "Verify the provider credentials in the load board settings.");
                    }

                    configuration.AccessToken = token.AccessToken;
                    configuration.RefreshToken = token.RefreshToken ?? configuration.RefreshToken;
                    configuration.TokenExpiresAt = token.ExpiresAt;
                    await tenantUow.SaveChangesAsync(ct);
                }
            }
            finally
            {
                refreshLock.Release();
            }
        }

        provider.Initialize(configuration);
        return Result<ILoadBoardProviderService>.Ok(provider);
    }

    private static bool HasValidToken(LoadBoardConfiguration configuration)
    {
        return !string.IsNullOrEmpty(configuration.AccessToken) &&
               configuration.TokenExpiresAt is { } expiresAt &&
               DateTime.UtcNow < expiresAt - ExpirySkew;
    }

    private async Task<OAuthTokenResultDto?> AcquireOrRefreshAsync(
        ILoadBoardProviderService provider, LoadBoardConfiguration configuration)
    {
        if (!string.IsNullOrEmpty(configuration.RefreshToken))
        {
            var refreshed = await provider.RefreshTokenAsync(configuration.RefreshToken);
            if (refreshed is not null)
            {
                return refreshed;
            }
        }

        logger.LogInformation("Acquiring new {Provider} token for configuration {ConfigurationId}",
            configuration.ProviderType, configuration.Id);
        return await provider.AcquireTokenAsync(configuration.ApiKey, configuration.ApiSecret);
    }
}
