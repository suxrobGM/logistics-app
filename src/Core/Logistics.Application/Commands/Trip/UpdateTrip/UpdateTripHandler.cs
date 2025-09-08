using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class UpdateTripHandler : IAppRequestHandler<UpdateTripCommand, Result>
{
    private readonly ILoadService _loads;
    private readonly ILogger<UpdateTripHandler> _log;
    private readonly ITenantUnitOfWork _uow;

    public UpdateTripHandler(ITenantUnitOfWork uow, ILoadService loads, ILogger<UpdateTripHandler> log)
    {
        _uow = uow;
        _loads = loads;
        _log = log;
    }

    public async Task<Result> Handle(UpdateTripCommand req, CancellationToken ct)
    {
        var trip = await _uow.Repository<Trip>().GetByIdAsync(req.TripId, ct);
        if (trip is null)
        {
            return Result.Fail($"Trip '{req.TripId}' not found.");
        }

        // Update only name if trip status is not draft
        if (trip.Status != TripStatus.Draft && !string.IsNullOrEmpty(req.Name))
        {
            trip.Name = req.Name;
            await _uow.SaveChangesAsync(ct);
            return Result.Ok();
        }

        if (trip.Status != TripStatus.Draft)
        {
            return Result.Fail("Only trips in 'Draft' status can be updated.");
        }

        // Basic fields
        if (!string.IsNullOrWhiteSpace(req.Name))
        {
            trip.Name = req.Name!;
        }

        // Truck swap (if requested)
        if (req.TruckId is { } newTruckId && newTruckId != trip.TruckId)
        {
            var newTruck = await _uow.Repository<Truck>().GetByIdAsync(newTruckId, ct);
            if (newTruck is null)
            {
                return Result.Fail($"Truck '{newTruckId}' not found.");
            }

            trip.TruckId = newTruck.Id;
            trip.Truck = newTruck;
        }

        // Compose final load set
        var loadsMap = trip.GetLoads().ToDictionary(l => l.Id);

        var removedCount = RemoveLoads(loadsMap, req.DetachLoadIds);

        var attachResult = await AttachExistingLoadsAsync(loadsMap, req.AttachLoadIds, trip, ct);
        if (!attachResult.Success)
        {
            return Result.Fail(attachResult.Error!);
        }

        var attachedCount = attachResult.Data;

        var created = await CreateNewLoadsAsync(req.NewLoads, trip.TruckId);
        foreach (var l in created)
        {
            loadsMap[l.Id] = l;
        }

        var createdCount = created.Count;

        // Rebuild stops from a final set
        trip.UpdateTripLoads(loadsMap.Values);

        await _uow.SaveChangesAsync(ct);

        _log.LogInformation(
            "Updated trip '{TripId}'. Name='{Name}', Truck='{TruckId}'. Loads={LoadCount} (attached {Attached}, created {Created}, removed {Removed})",
            trip.Id, trip.Name, trip.TruckId, loadsMap.Count, attachedCount, createdCount,
            removedCount);

        return Result.Ok();
    }

    private static int RemoveLoads(Dictionary<Guid, Load> loadsMap, IEnumerable<Guid>? loadIds)
    {
        if (loadIds is null)
        {
            return 0;
        }

        var before = loadsMap.Count;
        foreach (var id in loadIds.Distinct())
        {
            //var load = loadsMap[id];
            loadsMap.Remove(id);
            //_uow.Repository<Load>().Delete(load);
        }

        return before - loadsMap.Count;
    }

    private async Task<Result<int>> AttachExistingLoadsAsync(
        Dictionary<Guid, Load> map,
        IEnumerable<Guid>? attachIds,
        Trip trip,
        CancellationToken ct)
    {
        var count = 0;
        if (attachIds is null)
        {
            return Result<int>.Ok(count);
        }

        var ids = attachIds.Distinct().Where(id => !map.ContainsKey(id)).ToArray();
        if (ids.Length == 0)
        {
            return Result<int>.Ok(count);
        }

        var toAttach = await _uow.Repository<Load>().GetListAsync(l => ids.Contains(l.Id), ct);

        foreach (var load in toAttach)
        {
            if (load.Status == LoadStatus.Delivered)
            {
                return Result<int>.Fail($"Load '{load.Id}' is already delivered and cannot be attached.");
            }

            load.AssignedTruckId = trip.TruckId;
            load.AssignedTruck = trip.Truck;
            load.TripStop = null; // clear previous association

            map[load.Id] = load;
            count++;
        }

        return Result<int>.Ok(count);
    }

    private async Task<List<Load>> CreateNewLoadsAsync(
        IEnumerable<CreateTripLoadCommand>? commands,
        Guid truckId)
    {
        var result = new List<Load>();
        if (commands is null)
        {
            return result;
        }

        foreach (var c in commands)
        {
            var p = new CreateLoadParameters(
                c.Name,
                c.Type,
                (c.OriginAddress, c.OriginLocation),
                (c.DestinationAddress, c.DestinationLocation),
                c.DeliveryCost,
                c.Distance,
                c.CustomerId,
                truckId,
                c.AssignedDispatcherId);

            var entity = await _loads.CreateLoadAsync(p, false);
            result.Add(entity);
        }

        return result;
    }
}
