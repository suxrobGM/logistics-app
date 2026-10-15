using Logistics.Application.Abstractions.LoadBoard;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Services;

/// <remarks>
/// Token renewal is not locked: two requests that renew at once both get a working token and the
/// last write wins, which every supported board tolerates (none rotates its refresh credential).
/// </remarks>
internal sealed class LoadBoardProviderGateway(
    ITenantUnitOfWork tenantUow,
    ILoadBoardProviderFactory providerFactory,
    ILogger<LoadBoardProviderGateway> logger) : ILoadBoardProviderGateway
{
    private static readonly TimeSpan ExpirySkew = TimeSpan.FromMinutes(2);

    public async Task<Result<T>> ExecuteAsync<T>(
        LoadBoardConfiguration configuration,
        Func<ILoadBoardProviderService, Task<T>> call,
        CancellationToken ct = default)
    {
        var name = configuration.ProviderType.GetDescription();
        if (!providerFactory.IsProviderSupported(configuration.ProviderType))
        {
            return Result<T>.Fail($"{name} is not available yet.");
        }

        var provider = providerFactory.GetProvider(configuration.ProviderType);
        try
        {
            if (provider.RequiresOAuthToken && !HasValidToken(configuration))
            {
                await RenewTokenAsync(provider, configuration, ct);
            }

            provider.Initialize(configuration);
            try
            {
                return Result<T>.Ok(await call(provider));
            }
            catch (LoadBoardApiException ex) when (ex.IsUnauthorized && provider.RequiresOAuthToken)
            {
                logger.LogInformation("{Provider} rejected the access token for configuration {ConfigurationId}, re-authenticating",
                    configuration.ProviderType, configuration.Id);
                await RenewTokenAsync(provider, configuration, ct);
                return Result<T>.Ok(await call(provider));
            }
        }
        catch (LoadBoardApiException ex)
        {
            logger.LogWarning("{Provider} call failed for configuration {ConfigurationId}: {Error}",
                configuration.ProviderType, configuration.Id, ex.Message);
            return Result<T>.Fail($"{name}: {ex.Message}");
        }
    }

    public async Task<Result> ExecuteAsync(
        LoadBoardConfiguration configuration,
        Func<ILoadBoardProviderService, Task> call,
        CancellationToken ct = default)
    {
        var result = await ExecuteAsync(configuration, async provider =>
        {
            await call(provider);
            return true;
        }, ct);
        return result.IsSuccess ? Result.Ok() : Result.Fail(result.Error!);
    }

    public async Task<Result> TestConnectionAsync(LoadBoardConfiguration configuration, CancellationToken ct = default)
    {
        var name = configuration.ProviderType.GetDescription();
        if (!providerFactory.IsProviderSupported(configuration.ProviderType))
        {
            return Result.Fail($"{name} is not available yet.");
        }

        var provider = providerFactory.GetProvider(configuration.ProviderType);
        try
        {
            if (provider.RequiresOAuthToken)
            {
                await StoreTokenAsync(configuration, await provider.AcquireTokenAsync(configuration, ct), ct);
                return Result.Ok();
            }

            return await provider.ValidateCredentialsAsync(configuration, ct)
                ? Result.Ok()
                : Result.Fail($"{name} rejected the credentials. Check them and try again.");
        }
        catch (LoadBoardApiException ex)
        {
            return Result.Fail($"{name}: {ex.Message}");
        }
    }

    private static bool HasValidToken(LoadBoardConfiguration configuration)
    {
        return !string.IsNullOrEmpty(configuration.AccessToken) &&
               configuration.TokenExpiresAt is { } expiresAt &&
               DateTime.UtcNow < expiresAt - ExpirySkew;
    }

    /// <summary>
    /// Refresh first - cheaper, and for DAT it skips the service-account login - then a full
    /// acquisition when the refresh credential is missing or was itself rejected.
    /// </summary>
    private async Task RenewTokenAsync(
        ILoadBoardProviderService provider, LoadBoardConfiguration configuration, CancellationToken ct)
    {
        OAuthTokenResultDto? token = null;
        if (!string.IsNullOrEmpty(configuration.RefreshToken))
        {
            try
            {
                token = await provider.RefreshTokenAsync(configuration, ct);
            }
            catch (LoadBoardApiException ex) when (ex.IsUnauthorized)
            {
                logger.LogInformation("{Provider} refresh credential expired, acquiring a new token", configuration.ProviderType);
            }
        }

        token ??= await provider.AcquireTokenAsync(configuration, ct);
        await StoreTokenAsync(configuration, token, ct);
    }

    private async Task StoreTokenAsync(LoadBoardConfiguration configuration, OAuthTokenResultDto token, CancellationToken ct)
    {
        configuration.AccessToken = token.AccessToken;
        configuration.RefreshToken = token.RefreshToken ?? configuration.RefreshToken;
        configuration.TokenExpiresAt = token.ExpiresAt;

        // Harmless for a configuration still being created: nothing is tracked yet, and its
        // handler adds and saves it afterwards.
        await tenantUow.SaveChangesAsync(ct);
    }
}
