using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.FuelCards.Commands;

internal sealed class DeleteFuelCardProviderConfigurationHandler(
    ITenantUnitOfWork tenantUow,
    ILogger<DeleteFuelCardProviderConfigurationHandler> logger)
    : IAppRequestHandler<DeleteFuelCardProviderConfigurationCommand, Result>
{
    public async Task<Result> Handle(DeleteFuelCardProviderConfigurationCommand req, CancellationToken ct)
    {
        var config = await tenantUow.Repository<FuelCardProviderConfiguration>().GetByIdAsync(req.Id, ct);
        if (config is null)
        {
            return Result.Fail("Fuel card provider configuration not found");
        }

        tenantUow.Repository<FuelCardProviderConfiguration>().Delete(config);
        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation("Deleted fuel card provider configuration for {ProviderType}", config.ProviderType);
        return Result.Ok();
    }
}
