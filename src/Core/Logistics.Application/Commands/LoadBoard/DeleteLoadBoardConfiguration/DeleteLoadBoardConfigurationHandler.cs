using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class DeleteLoadBoardConfigurationHandler(
    ITenantUnitOfWork tenantUow,
    ILogger<DeleteLoadBoardConfigurationHandler> logger)
    : IAppRequestHandler<DeleteLoadBoardConfigurationCommand, Result>
{
    public async Task<Result> Handle(DeleteLoadBoardConfigurationCommand req, CancellationToken ct)
    {
        var config = await tenantUow.Repository<LoadBoardConfiguration>()
            .GetByIdAsync(req.Id, ct);

        if (config is null)
        {
            return Result.Fail("Load board configuration not found");
        }

        tenantUow.Repository<LoadBoardConfiguration>().Delete(config);
        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation("Deleted load board configuration for {ProviderType}", config.ProviderType);
        return Result.Ok();
    }
}
