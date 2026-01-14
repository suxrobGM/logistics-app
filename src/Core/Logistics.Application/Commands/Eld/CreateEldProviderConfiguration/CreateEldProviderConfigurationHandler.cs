using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class CreateEldProviderConfigurationHandler(
    ITenantUnitOfWork tenantUow,
    IEldProviderFactory eldProviderFactory,
    ILogger<CreateEldProviderConfigurationHandler> logger)
    : IAppRequestHandler<CreateEldProviderConfigurationCommand, Result>
{
    public async Task<Result> Handle(CreateEldProviderConfigurationCommand req, CancellationToken ct)
    {
        // Check if provider is supported
        if (!eldProviderFactory.IsProviderSupported(req.ProviderType))
        {
            return Result.Fail($"ELD provider '{req.ProviderType}' is not supported");
        }

        // Check if configuration already exists for this provider
        var existingConfig = await tenantUow.Repository<EldProviderConfiguration>()
            .GetAsync(c => c.ProviderType == req.ProviderType, ct);

        if (existingConfig is not null)
        {
            return Result.Fail(
                $"Configuration for {req.ProviderType} already exists. Please update the existing configuration.");
        }

        // Validate credentials with the provider
        var providerService = eldProviderFactory.GetProvider(req.ProviderType);
        var isValid = await providerService.ValidateCredentialsAsync(req.ApiKey, req.ApiSecret);

        if (!isValid)
        {
            return Result.Fail("Invalid API credentials. Please verify your API key and try again.");
        }

        // Create the configuration
        var config = new EldProviderConfiguration
        {
            ProviderType = req.ProviderType,
            ApiKey = req.ApiKey,
            ApiSecret = req.ApiSecret,
            WebhookSecret = req.WebhookSecret,
            IsActive = true
        };

        await tenantUow.Repository<EldProviderConfiguration>().AddAsync(config, ct);
        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation("Created ELD provider configuration for {ProviderType}", req.ProviderType);
        return Result.Ok();
    }
}
