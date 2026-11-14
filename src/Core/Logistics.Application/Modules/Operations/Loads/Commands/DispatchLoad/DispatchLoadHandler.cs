using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.ApplicationServices;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

internal sealed class DispatchLoadHandler(
    ITenantUnitOfWork tenantUow,
    IDispatchEligibilityService eligibilityService)
    : IRequestHandler<DispatchLoadCommand, Result>
{
    public async Task<Result> Handle(DispatchLoadCommand req, CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.Id, ct);

        if (load is null)
        {
            return Result.Fail($"Load not found with ID '{req.Id}'", ErrorCodes.NotFound);
        }

        if (load.AssignedTruckId is not { } truckId)
        {
            return Result.Fail($"Assign a truck to load #{load.Number} before dispatching it");
        }

        // Eligibility is enforced at the dispatch gate - assignment up to this point is
        // planning. The truck and driver must be qualified to carry the load
        // (license, ADR/Hazmat endorsements, ADR cert, placarding).
        var eligibility = await eligibilityService.CheckAsync(truckId, load.Id, ct: ct);
        if (!eligibility.IsEligible)
        {
            var reasons = string.Join("; ",
                eligibility.Issues
                    .Where(i => i.Severity == EligibilitySeverity.Error)
                    .Select(i => i.Message));
            return Result.Fail($"Cannot dispatch load: {reasons}");
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
