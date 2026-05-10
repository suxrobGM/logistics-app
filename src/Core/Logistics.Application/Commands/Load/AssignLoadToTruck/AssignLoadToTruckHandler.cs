using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class AssignLoadToTruckHandler(
    ITenantUnitOfWork tenantUow,
    IDispatchEligibilityService eligibilityService)
    : IAppRequestHandler<AssignLoadToTruckCommand, Result>
{
    public async Task<Result> Handle(AssignLoadToTruckCommand req, CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.LoadId, ct);
        if (load is null)
        {
            return Result.Fail($"Load not found with ID '{req.LoadId}'");
        }

        // Run dispatch eligibility check before mutating state. Skipped only when
        // unassigning a truck (TruckId == null).
        if (req.TruckId.HasValue)
        {
            var eligibility = await eligibilityService.CheckAsync(req.TruckId.Value, req.LoadId, ct: ct);
            if (!eligibility.IsEligible)
            {
                var blockingReasons = string.Join("; ",
                    eligibility.Issues
                        .Where(i => i.Severity == EligibilitySeverity.Error)
                        .Select(i => i.Message));
                return Result.Fail($"Cannot assign load to truck: {blockingReasons}");
            }
        }

        load.AssignedTruckId = req.TruckId;
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
