using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class DispatchTripHandler(
    ITenantUnitOfWork tenantUow,
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
