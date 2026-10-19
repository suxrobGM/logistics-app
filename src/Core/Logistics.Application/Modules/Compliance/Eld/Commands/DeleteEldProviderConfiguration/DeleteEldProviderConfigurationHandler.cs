using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Compliance.Eld.Commands;

internal sealed class DeleteEldProviderConfigurationHandler(
    ITenantUnitOfWork tenantUow,
    ILogger<DeleteEldProviderConfigurationHandler> logger)
    : IRequestHandler<DeleteEldProviderConfigurationCommand, Result>
{
    public async Task<Result> Handle(DeleteEldProviderConfigurationCommand req, CancellationToken ct)
    {
        var config = await tenantUow.Repository<EldProviderConfiguration>()
            .GetByIdAsync(req.ProviderId, ct);

        if (config is null)
        {
            return Result.Fail("ELD provider configuration not found");
        }

        // Delete the HOS statuses of the drivers this provider maps
        var mappedEmployeeIds = (await tenantUow.Repository<EldDriverMapping>()
                .GetListAsync(m => m.ProviderType == config.ProviderType, ct))
            .Select(m => m.EmployeeId)
            .ToList();
        var hosStatuses = await tenantUow.Repository<DriverHosStatus>()
            .GetListAsync(h => mappedEmployeeIds.Contains(h.EmployeeId), ct);

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
