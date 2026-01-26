using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class CancelTripHandler(
    ITenantUnitOfWork tenantUow,
    ILogger<CancelTripHandler> logger)
    : IAppRequestHandler<CancelTripCommand, Result>
{
    public async Task<Result> Handle(CancelTripCommand req, CancellationToken ct)
    {
        var trip = await tenantUow.Repository<Trip>().GetByIdAsync(req.TripId, ct);

        if (trip is null)
        {
            return Result.Fail($"Could not find the trip with ID '{req.TripId}'");
        }

        try
        {
            trip.Cancel();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message);
        }

        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Cancelled trip '{TripName}' with ID '{TripId}'. Reason: {Reason}",
            trip.Name, trip.Id, req.Reason ?? "Not specified");

        return Result.Ok();
    }
}
