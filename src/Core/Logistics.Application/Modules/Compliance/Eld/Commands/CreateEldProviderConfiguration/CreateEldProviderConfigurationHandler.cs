using Logistics.Application.Abstractions.Eld;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Compliance.Eld.Commands;

internal sealed class CreateEldProviderConfigurationHandler(
    ITenantUnitOfWork tenantUow,
    IEldProviderFactory eldProviderFactory,
    ILogger<CreateEldProviderConfigurationHandler> logger)
    : IRequestHandler<CreateEldProviderConfigurationCommand, Result>
{
    private static readonly string[] SamsaraRegions = ["us", "eu", "ca"];

    public async Task<Result> Handle(CreateEldProviderConfigurationCommand req, CancellationToken ct)
    {
        if (!eldProviderFactory.IsProviderSupported(req.ProviderType))
        {
            return Result.Fail($"ELD provider '{req.ProviderType}' is not supported");
        }

        var externalAccountId = string.IsNullOrWhiteSpace(req.ExternalAccountId) ? null : req.ExternalAccountId.Trim();
        if (req.ProviderType == EldProviderType.TtEld && externalAccountId is null)
        {
            return Result.Fail("TT ELD needs the carrier's USDOT number.");
        }

        var region = req.ProviderType == EldProviderType.Samsara ? req.Region?.Trim().ToLowerInvariant() : null;
        if (region is not null && !SamsaraRegions.Contains(region))
        {
            return Result.Fail("Samsara region must be us, eu or ca.");
        }

        var existingConfig = await tenantUow.Repository<EldProviderConfiguration>()
            .GetAsync(c => c.ProviderType == req.ProviderType, ct);

        if (existingConfig is not null)
        {
            return Result.Fail(
                $"Configuration for {req.ProviderType} already exists. Please update the existing configuration.");
        }

        var config = new EldProviderConfiguration
        {
            ProviderType = req.ProviderType,
            ApiKey = req.ApiKey,
            ApiSecret = req.ApiSecret,
            WebhookSecret = string.IsNullOrWhiteSpace(req.WebhookSecret) ? null : req.WebhookSecret.Trim(),
            ExternalAccountId = externalAccountId,
            Region = region,
            IsActive = true
        };

        // Initialized from the unsaved config so validation hits the right host (Samsara region) and
        // account (TT ELD's USDOT in the URL)
        var providerService = eldProviderFactory.GetProvider(config);
        if (!await providerService.ValidateCredentialsAsync(req.ApiKey, req.ApiSecret))
        {
            return Result.Fail("Invalid API credentials. Please verify your API key and try again.");
        }

        await tenantUow.Repository<EldProviderConfiguration>().AddAsync(config, ct);
        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation("Created ELD provider configuration for {ProviderType}", req.ProviderType);
        return Result.Ok();
    }
}
