using Logistics.Application.Modules.Integrations.LoadBoard.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Commands;

internal sealed class UpdateLoadBoardConfigurationHandler(
    ITenantUnitOfWork tenantUow,
    ILoadBoardProviderGateway gateway)
    : IRequestHandler<UpdateLoadBoardConfigurationCommand, Result>
{
    public async Task<Result> Handle(UpdateLoadBoardConfigurationCommand req, CancellationToken ct)
    {
        var config = await tenantUow.Repository<LoadBoardConfiguration>().GetByIdAsync(req.Id, ct);
        if (config is null)
        {
            return Result.Fail("Load board provider not found");
        }

        var credentialsChanged = false;
        if (!string.IsNullOrWhiteSpace(req.ApiKey) && req.ApiKey.Trim() != config.ApiKey)
        {
            config.ApiKey = req.ApiKey.Trim();
            credentialsChanged = true;
        }

        if (!string.IsNullOrEmpty(req.ApiSecret))
        {
            config.ApiSecret = req.ApiSecret;
            credentialsChanged = true;
        }

        if (req.UserEmail is not null && req.UserEmail.Trim() != (config.UserEmail ?? string.Empty))
        {
            config.UserEmail = req.UserEmail.Trim() is { Length: > 0 } email ? email : null;
            credentialsChanged = true;
        }

        config.IsActive = req.IsActive ?? config.IsActive;
        config.MarketRatesEnabled = req.MarketRatesEnabled ?? config.MarketRatesEnabled;
        config.CompanyDotNumber = req.CompanyDotNumber ?? config.CompanyDotNumber;
        config.CompanyMcNumber = req.CompanyMcNumber ?? config.CompanyMcNumber;

        if (credentialsChanged)
        {
            // Tokens belong to the old credentials. The test saves the scope only on success, so
            // a rejected edit leaves the stored connection as it was.
            config.AccessToken = null;
            config.RefreshToken = null;
            config.TokenExpiresAt = null;

            var connection = await gateway.TestConnectionAsync(config, ct);
            if (!connection.IsSuccess)
            {
                return connection;
            }
        }

        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
