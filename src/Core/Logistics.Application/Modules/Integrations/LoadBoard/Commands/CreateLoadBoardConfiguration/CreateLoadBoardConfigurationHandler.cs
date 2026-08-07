using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.LoadBoard;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Commands;

internal sealed class CreateLoadBoardConfigurationHandler(
    ITenantUnitOfWork tenantUow,
    ILoadBoardProviderFactory providerFactory,
    ILogger<CreateLoadBoardConfigurationHandler> logger)
    : IAppRequestHandler<CreateLoadBoardConfigurationCommand, Result>
{
    public async Task<Result> Handle(CreateLoadBoardConfigurationCommand req, CancellationToken ct)
    {
        // Check if provider is supported
        if (!providerFactory.IsProviderSupported(req.ProviderType))
        {
            return Result.Fail($"Load board provider '{req.ProviderType}' is not supported");
        }

        // Check if configuration already exists for this provider
        var existingConfig = await tenantUow.Repository<LoadBoardConfiguration>()
            .GetAsync(c => c.ProviderType == req.ProviderType, ct);

        if (existingConfig is not null)
        {
            return Result.Fail(
                $"Configuration for {req.ProviderType} already exists. Please update the existing configuration.");
        }

        var config = new LoadBoardConfiguration
        {
            ProviderType = req.ProviderType,
            ApiKey = req.ApiKey,
            ApiSecret = req.ApiSecret,
            WebhookSecret = req.WebhookSecret,
            CompanyDotNumber = req.CompanyDotNumber,
            CompanyMcNumber = req.CompanyMcNumber,
            IsActive = true
        };

        // OAuth providers validate by acquiring a token, which must be stored or every
        // later call goes out unauthenticated. Key-based providers just validate.
        var providerService = providerFactory.GetProvider(req.ProviderType);
        if (providerService.RequiresOAuthToken)
        {
            var token = await providerService.AcquireTokenAsync(req.ApiKey, req.ApiSecret);
            if (token is null)
            {
                return Result.Fail("Invalid API credentials. Please verify your API key and try again.");
            }

            config.AccessToken = token.AccessToken;
            config.RefreshToken = token.RefreshToken;
            config.TokenExpiresAt = token.ExpiresAt;
        }
        else if (!await providerService.ValidateCredentialsAsync(req.ApiKey, req.ApiSecret))
        {
            return Result.Fail("Invalid API credentials. Please verify your API key and try again.");
        }

        await tenantUow.Repository<LoadBoardConfiguration>().AddAsync(config, ct);
        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation("Created load board provider configuration for {ProviderType}", req.ProviderType);
        return Result.Ok();
    }
}
