using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

internal sealed class AssignLoadToTruckHandler(ITenantUnitOfWork tenantUow) : IRequestHandler<AssignLoadToTruckCommand, Result>
{
    public async Task<Result> Handle(AssignLoadToTruckCommand req, CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.LoadId, ct);

        if (load is null)
        {
            return Result.Fail($"Load not found with ID '{req.LoadId}'");
        }

        if (req.TruckId != load.AssignedTruckId && load.TripStops.FirstOrDefault() is { } stop)
        {
            return Result.Fail(
                $"Load #{load.Number} is on trip #{stop.Trip.Number}, which sets its truck. Change the trip's truck instead.");
        }

        if (req.TruckId is { } truckId)
        {
            var truck = await tenantUow.Repository<Truck>().GetByIdAsync(truckId, ct);
            if (truck is null)
            {
                return Result.Fail($"Truck not found with ID '{truckId}'");
            }

            if (TruckLoadCompatibility.DescribeMismatch(truck.Number, truck.Type, load.Type) is { } mismatch)
            {
                return Result.Fail(mismatch);
            }
        }

        load.AssignedTruckId = req.TruckId;
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
