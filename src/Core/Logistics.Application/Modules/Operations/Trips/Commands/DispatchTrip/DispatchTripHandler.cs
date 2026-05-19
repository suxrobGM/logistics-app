using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Dispatch;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Operations.Trips.Commands;

internal sealed class DispatchTripHandler(
    ITenantUnitOfWork tenantUow,
    IDispatchEligibilityService eligibilityService,
    ILogger<DispatchTripHandler> logger)
    : IAppRequestHandler<DispatchTripCommand, Result>
{
    public async Task<Result> Handle(DispatchTripCommand req, CancellationToken ct)
    {
        var trip = await tenantUow.Repository<Trip>().GetByIdAsync(req.TripId, ct);

        if (trip is null)
        {
            return Result.Fail($"Could not find the trip with ID '{req.TripId}'");
        }

        // Hard eligibility gate at dispatch â€” every load on the trip must be carryable
        // by the assigned truck/driver. Planning steps (assignment, trip creation) don't
        // enforce this; the dispatcher is the final commit.
        if (trip.TruckId.HasValue)
        {
            // Distinct load IDs across stops (a load may have both pickup + dropoff stops).
            var loadIds = trip.Stops.Select(s => s.LoadId).Distinct();
            foreach (var loadId in loadIds)
            {
                var eligibility = await eligibilityService.CheckAsync(
                    trip.TruckId.Value, loadId, ct: ct);
                if (!eligibility.IsEligible)
                {
                    var reasons = string.Join("; ",
                        eligibility.Issues
                            .Where(i => i.Severity == EligibilitySeverity.Error)
                            .Select(i => i.Message));
                    return Result.Fail(
                        $"Cannot dispatch trip â€” load '{loadId}' not eligible: {reasons}");
                }
            }
        }

        try
        {
            trip.Dispatch();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message);
        }

        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Dispatched trip '{TripName}' with ID '{TripId}'",
            trip.Name, trip.Id);

        return Result.Ok();
    }
}
