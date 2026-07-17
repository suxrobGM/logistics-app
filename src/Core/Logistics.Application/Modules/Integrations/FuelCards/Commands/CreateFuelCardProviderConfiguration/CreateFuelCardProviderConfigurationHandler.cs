using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.FuelCards;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.FuelCards.Commands;

internal sealed class CreateFuelCardProviderConfigurationHandler(
    ITenantUnitOfWork tenantUow,
    IFuelCardProviderFactory providerFactory,
    ILogger<CreateFuelCardProviderConfigurationHandler> logger)
    : IAppRequestHandler<CreateFuelCardProviderConfigurationCommand, Result>
{
    public async Task<Result> Handle(CreateFuelCardProviderConfigurationCommand req, CancellationToken ct)
    {
        if (!providerFactory.IsProviderSupported(req.ProviderType))
        {
            return Result.Fail($"Fuel card provider '{req.ProviderType}' is not supported");
        }

        var existingConfig = await tenantUow.Repository<FuelCardProviderConfiguration>()
            .GetAsync(c => c.ProviderType == req.ProviderType, ct);

        if (existingConfig is not null)
        {
            return Result.Fail(
                $"Configuration for {req.ProviderType} already exists. Please delete the existing configuration first.");
        }

        var providerService = providerFactory.GetProvider(req.ProviderType);
        var isValid = await providerService.ValidateCredentialsAsync(req.ApiKey, req.ApiSecret);

        if (!isValid)
        {
            return Result.Fail("Invalid API credentials. Please verify your API key and try again.");
        }

        var config = new FuelCardProviderConfiguration
        {
            ProviderType = req.ProviderType,
            ApiKey = req.ApiKey,
            ApiSecret = req.ApiSecret,
            ExternalAccountId = req.ExternalAccountId,
            IsActive = true
        };

        await tenantUow.Repository<FuelCardProviderConfiguration>().AddAsync(config, ct);
        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation("Created fuel card provider configuration for {ProviderType}", req.ProviderType);
        return Result.Ok();
    }
}
