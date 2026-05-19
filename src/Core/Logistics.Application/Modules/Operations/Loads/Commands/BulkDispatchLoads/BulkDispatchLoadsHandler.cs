using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Dispatch;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

internal sealed class BulkDispatchLoadsHandler(
    ITenantUnitOfWork tenantUow,
    IDispatchEligibilityService eligibilityService)
    : IAppRequestHandler<BulkDispatchLoadsCommand, Result>
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
        var ineligibleReasons = new List<string>();

        foreach (var load in loads)
        {
            if (load.Status != LoadStatus.Draft)
            {
                skippedCount++;
                continue;
            }

            if (load.AssignedTruckId.HasValue)
            {
                var eligibility = await eligibilityService.CheckAsync(
                    load.AssignedTruckId.Value, load.Id, ct: ct);
                if (!eligibility.IsEligible)
                {
                    var reasons = string.Join(", ",
                        eligibility.Issues
                            .Where(i => i.Severity == EligibilitySeverity.Error)
                            .Select(i => i.Message));
                    ineligibleReasons.Add($"Load '{load.Name}': {reasons}");
                    skippedCount++;
                    continue;
                }
            }

            load.Dispatch();
            dispatchedCount++;
        }

        await tenantUow.SaveChangesAsync(ct);

        if (dispatchedCount == 0)
        {
            var detail = ineligibleReasons.Count > 0
                ? $" Eligibility issues: {string.Join("; ", ineligibleReasons)}"
                : " Only loads in Draft status can be dispatched.";
            return Result.Fail($"No loads were dispatched.{detail}");
        }

        return Result.Ok();
    }
}
