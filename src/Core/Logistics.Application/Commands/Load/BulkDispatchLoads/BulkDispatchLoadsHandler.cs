using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class BulkDispatchLoadsHandler(ITenantUnitOfWork tenantUow) : IAppRequestHandler<BulkDispatchLoadsCommand, Result>
{
    public async Task<Result> Handle(BulkDispatchLoadsCommand req, CancellationToken ct)
    {
        var loads = await tenantUow.Repository<Load>()
            .GetListAsync(l => req.LoadIds.Contains(l.Id), ct);

        if (loads.Count == 0)
        {
            return Result.Fail("No loads found with the provided IDs");
        }

        var dispatchedCount = 0;
        var skippedCount = 0;

        foreach (var load in loads)
        {
            if (load.Status == LoadStatus.Draft)
            {
                load.Dispatch();
                dispatchedCount++;
            }
            else
            {
                skippedCount++;
            }
        }

        await tenantUow.SaveChangesAsync(ct);

        if (skippedCount > 0 && dispatchedCount == 0)
        {
            return Result.Fail("No loads were dispatched. Only loads in Draft status can be dispatched.");
        }

        return Result.Ok();
    }
}
