using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Dispatch;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

internal sealed class DispatchLoadHandler(
    ITenantUnitOfWork tenantUow,
    IDispatchEligibilityService eligibilityService)
    : IAppRequestHandler<DispatchLoadCommand, Result>
{
    public async Task<Result> Handle(DispatchLoadCommand req, CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.Id, ct);

        if (load is null)
        {
            return Result.Fail($"Load not found with ID '{req.Id}'");
        }

        // Eligibility is enforced at the dispatch gate â€” assignment up to this point is
        // planning. If the load is being dispatched, the truck and driver must be qualified
        // to carry it (license, ADR/Hazmat endorsements, ADR cert, placarding).
        if (load.AssignedTruckId.HasValue)
        {
            var eligibility = await eligibilityService.CheckAsync(
                load.AssignedTruckId.Value, load.Id, ct: ct);
            if (!eligibility.IsEligible)
            {
                var reasons = string.Join("; ",
                    eligibility.Issues
                        .Where(i => i.Severity == EligibilitySeverity.Error)
                        .Select(i => i.Message));
                return Result.Fail($"Cannot dispatch load: {reasons}");
            }
        }

        try
        {
            load.Dispatch();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message);
        }

        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
