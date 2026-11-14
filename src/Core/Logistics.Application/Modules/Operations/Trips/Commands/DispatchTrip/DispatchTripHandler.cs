using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.ApplicationServices;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Operations.Trips.Commands;

internal sealed class DispatchTripHandler(
    ITenantUnitOfWork tenantUow,
    IDispatchEligibilityService eligibilityService,
    ILogger<DispatchTripHandler> logger)
    : IRequestHandler<DispatchTripCommand, Result>
{
    public async Task<Result> Handle(DispatchTripCommand req, CancellationToken ct)
    {
        var trip = await tenantUow.Repository<Trip>().GetByIdAsync(req.TripId, ct);

        if (trip is null)
        {
            return Result.Fail($"Could not find the trip with ID '{req.TripId}'", ErrorCodes.NotFound);
        }

        if (trip.TruckId is not { } truckId)
        {
            return Result.Fail($"Assign a truck to trip '{trip.Name}' before dispatching it");
        }

        // Hard eligibility gate at dispatch - every load on the trip must be carryable
        // by the assigned truck/driver. Planning steps (assignment, trip creation) don't
        // enforce this; the dispatcher is the final commit.
        // Distinct load IDs across stops (a load may have both pickup + dropoff stops).
        foreach (var loadId in trip.Stops.Select(s => s.LoadId).Distinct())
        {
            var eligibility = await eligibilityService.CheckAsync(truckId, loadId, ct: ct);
            if (!eligibility.IsEligible)
            {
                var reasons = string.Join("; ",
                    eligibility.Issues
                        .Where(i => i.Severity == EligibilitySeverity.Error)
                        .Select(i => i.Message));
                return Result.Fail(
                    $"Cannot dispatch trip - load '{loadId}' not eligible: {reasons}");
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
