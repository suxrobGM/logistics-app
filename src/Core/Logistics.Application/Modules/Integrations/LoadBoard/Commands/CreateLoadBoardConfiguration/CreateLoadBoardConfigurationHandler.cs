using Logistics.Application.Modules.Integrations.LoadBoard.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Commands;

internal sealed class CreateLoadBoardConfigurationHandler(
    ITenantUnitOfWork tenantUow,
    ILoadBoardProviderGateway gateway,
    ILogger<CreateLoadBoardConfigurationHandler> logger)
    : IRequestHandler<CreateLoadBoardConfigurationCommand, Result>
{
    public async Task<Result> Handle(CreateLoadBoardConfigurationCommand req, CancellationToken ct)
    {
        var existingConfig = await tenantUow.Repository<LoadBoardConfiguration>()
            .GetAsync(c => c.ProviderType == req.ProviderType, ct);

        if (existingConfig is not null)
        {
            return Result.Fail(
                $"{req.ProviderType.GetDescription()} is already connected. Edit the existing connection instead.");
        }

        var config = new LoadBoardConfiguration
        {
            ProviderType = req.ProviderType,
            ApiKey = req.ApiKey.Trim(),
            ApiSecret = req.ApiSecret,
            UserEmail = string.IsNullOrWhiteSpace(req.UserEmail) ? null : req.UserEmail.Trim(),
            MarketRatesEnabled = req.MarketRatesEnabled,
            CompanyDotNumber = req.CompanyDotNumber,
            CompanyMcNumber = req.CompanyMcNumber,
            IsActive = true
        };

        // Fails with "not available yet" for a board we have no provider for. Token providers prove
        // the credentials by acquiring one, and keep it for the first call.
        var connection = await gateway.TestConnectionAsync(config, ct);
        if (!connection.IsSuccess)
        {
            return connection;
        }

        await tenantUow.Repository<LoadBoardConfiguration>().AddAsync(config, ct);
        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation("Created load board provider configuration for {ProviderType}", req.ProviderType);
        return Result.Ok();
    }
}
