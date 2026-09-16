using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Operations.Trips.Commands;

internal sealed class MarkStopArrivedHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser,
    ILogger<MarkStopArrivedHandler> logger)
    : IRequestHandler<MarkStopArrivedCommand, Result>
{
    public async Task<Result> Handle(MarkStopArrivedCommand req, CancellationToken ct)
    {
        var trip = await tenantUow.Repository<Trip>().GetByIdAsync(req.TripId, ct);

        if (trip is null)
        {
            return Result.Fail($"Could not find the trip with ID '{req.TripId}'");
        }

        if (!currentUser.CanDrive(trip.Truck))
        {
            return Result.Fail("This trip isn't assigned to your truck.");
        }

        try
        {
            trip.MarkStopArrived(req.StopId);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message);
        }

        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Marked stop '{StopId}' as arrived for trip '{TripName}' (ID: {TripId})",
            req.StopId, trip.Name, trip.Id);

        return Result.Ok();
    }
}
