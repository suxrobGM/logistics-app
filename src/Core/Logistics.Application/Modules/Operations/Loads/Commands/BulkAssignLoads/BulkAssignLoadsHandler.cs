using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

internal sealed class BulkAssignLoadsHandler(ITenantUnitOfWork tenantUow) : IRequestHandler<BulkAssignLoadsCommand, Result>
{
    public async Task<Result> Handle(BulkAssignLoadsCommand req, CancellationToken ct)
    {
        var loads = await tenantUow.Repository<Load>()
            .GetListAsync(l => req.LoadIds.Contains(l.Id), ct);

        if (loads.Count == 0)
        {
            return Result.Fail("No loads found with the provided IDs");
        }

        var onTrip = await tenantUow.Repository<Load>().Query()
            .Where(l => req.LoadIds.Contains(l.Id) && l.AssignedTruckId != req.TruckId && l.TripStops.Any())
            .Select(l => l.Number)
            .ToListAsync(ct);

        if (onTrip.Count > 0)
        {
            var numbers = string.Join(", ", onTrip.Order().Select(n => $"#{n}"));
            return Result.Fail($"Loads {numbers} are on a trip, which sets their truck. Change the trip's truck instead.");
        }

        var truck = await tenantUow.Repository<Truck>().GetByIdAsync(req.TruckId, ct);
        if (truck is null)
        {
            return Result.Fail($"Truck not found with ID '{req.TruckId}'");
        }

        foreach (var load in loads.Where(l => l.AssignedTruckId != truck.Id))
        {
            load.AssignToTruck(truck);
        }

        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
