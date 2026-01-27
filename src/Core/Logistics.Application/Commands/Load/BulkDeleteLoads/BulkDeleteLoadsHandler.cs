using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class BulkDeleteLoadsHandler(ITenantUnitOfWork tenantUow) : IAppRequestHandler<BulkDeleteLoadsCommand, Result>
{
    public async Task<Result> Handle(BulkDeleteLoadsCommand req, CancellationToken ct)
    {
        var loads = await tenantUow.Repository<Load>()
            .GetListAsync(l => req.LoadIds.Contains(l.Id), ct);

        if (loads.Count == 0)
        {
            return Result.Fail("No loads found with the provided IDs");
        }

        var deletedCount = 0;
        var skippedCount = 0;

        foreach (var load in loads)
        {
            if (load.Status == LoadStatus.Draft)
            {
                load.MarkRemovedFromTruck();
                tenantUow.Repository<Load>().Delete(load);
                deletedCount++;
            }
            else
            {
                skippedCount++;
            }
        }

        await tenantUow.SaveChangesAsync(ct);

        if (skippedCount > 0 && deletedCount == 0)
        {
            return Result.Fail("No loads were deleted. Only loads in Draft status can be deleted.");
        }

        return Result.Ok();
    }
}
