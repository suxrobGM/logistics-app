using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class DeleteEldProviderConfigurationHandler(
    ITenantUnitOfWork tenantUow,
    ILogger<DeleteEldProviderConfigurationHandler> logger)
    : IAppRequestHandler<DeleteEldProviderConfigurationCommand, Result>
{
    public async Task<Result> Handle(DeleteEldProviderConfigurationCommand req, CancellationToken ct)
    {
        var config = await tenantUow.Repository<EldProviderConfiguration>()
            .GetByIdAsync(req.ProviderId, ct);

        if (config is null)
        {
            return Result.Fail("ELD provider configuration not found");
        }

        // Delete related driver mappings
        var mappings = await tenantUow.Repository<EldDriverMapping>()
            .GetListAsync(m => m.ProviderType == config.ProviderType, ct);

        foreach (var mapping in mappings)
        {
            tenantUow.Repository<EldDriverMapping>().Delete(mapping);
        }

        // Delete related HOS statuses
        var hosStatuses = await tenantUow.Repository<DriverHosStatus>()
            .GetListAsync(h => h.ProviderType == config.ProviderType, ct);

        foreach (var hosStatus in hosStatuses)
        {
            tenantUow.Repository<DriverHosStatus>().Delete(hosStatus);
        }

        // Delete the provider configuration
        tenantUow.Repository<EldProviderConfiguration>().Delete(config);

        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation("Deleted ELD provider configuration {ProviderId} ({ProviderType})",
            req.ProviderId, config.ProviderType);

        return Result.Ok();
    }
}
