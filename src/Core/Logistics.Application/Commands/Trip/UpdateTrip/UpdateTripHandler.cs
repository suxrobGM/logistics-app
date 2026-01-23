using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class UpdateTripHandler(
    ITenantUnitOfWork uow,
    ILoadService loadService,
    ILogger<UpdateTripHandler> logger,
    IPushNotificationService pushNotificationService,
    INotificationService notificationService)
    : IAppRequestHandler<UpdateTripCommand, Result>
{
    public async Task<Result> Handle(UpdateTripCommand req, CancellationToken ct)
    {
        var trip = await uow.Repository<Trip>().GetByIdAsync(req.TripId, ct);
        if (trip is null)
        {
            return Result.Fail($"Trip '{req.TripId}' not found.");
        }

        if (trip.Status != TripStatus.Draft)
        {
            return Result.Fail("Only trips in 'Draft' status can be updated.");
        }

        // Name field
        if (!string.IsNullOrEmpty(req.Name) && trip.Name != req.Name)
        {
            trip.Name = req.Name;
        }

        // Track old and new truck for notifications
        var oldTruck = trip.Truck;
        Truck? newTruck = null;

        // Truck swap
        if (req.TruckId is { } newTruckId && newTruckId != trip.TruckId)
        {
            newTruck = await uow.Repository<Truck>().GetByIdAsync(newTruckId, ct);
            if (newTruck is null)
            {
                return Result.Fail($"Truck '{newTruckId}' not found.");
            }

            trip.TruckId = newTruck.Id;
            trip.Truck = newTruck;
        }

        // A map of current loads on the trip for easy access, key is the load ID and value is the load entity
        var loadsMap = trip.GetLoads().ToDictionary(l => l.Id);

        var removedCount = RemoveLoads(trip, loadsMap, req.DetachedLoadIds);

        // Disabled the attach logic for now
        // var attachResult = await AttachExistingLoadsAsync(trip, loadsMap, req.AttachedLoadIds, ct);
        // if (!attachResult.Success)
        // {
        //     return Result.Fail(attachResult.Error!);
        // }

        var attachedCount = 0; // attachResult.Data;
        var createdCount = await CreateNewLoadsAsync(trip, loadsMap, req.NewLoads);

        // Update stop order if optimized stops are provided
        if (req.OptimizedStops != null && req.OptimizedStops.Any())
        {
            UpdateStopOrder(trip, req.OptimizedStops);
        }

        // Always update total distance when provided (from route optimizer)
        if (req.TotalDistance.HasValue)
        {
            trip.TotalDistance = req.TotalDistance.Value;
        }

        await uow.SaveChangesAsync(ct);

        // Send notifications for truck assignment changes
        await NotifyTruckAssignmentAsync(oldTruck, newTruck, trip);

        logger.LogInformation(
            "Updated trip '{TripId}'. Name='{Name}', Truck='{TruckId}'. Loads={LoadCount} (attached {Attached}, created {Created}, removed {Removed})",
            trip.Id, trip.Name, trip.TruckId, loadsMap.Count, attachedCount, createdCount,
            removedCount);

        return Result.Ok();
    }

    private async Task NotifyTruckAssignmentAsync(Truck? oldTruck, Truck? newTruck, Trip trip)
    {
        if (newTruck != null && oldTruck == null)
        {
            // First-time truck assignment - trip was created without truck and now assigned
            await pushNotificationService.SendNotificationAsync(
                "New trip assigned",
                $"Trip #{trip.Number} '{trip.Name}' has been assigned to you",
                newTruck.MainDriver?.DeviceToken ?? string.Empty);

            // Send in-app notification for TMS portal users
            var driverName = newTruck.MainDriver?.GetFullName() ?? newTruck.Number;
            await notificationService.SendNotificationAsync(
                "Trip assigned to truck",
                $"Trip #{trip.Number} has been assigned to {driverName}");
        }
        else if (newTruck != null && oldTruck != null && oldTruck.Id != newTruck.Id)
        {
            // Truck was switched
            await pushNotificationService.SendNotificationAsync(
                "New trip assigned",
                $"Trip #{trip.Number} '{trip.Name}' has been assigned to you",
                newTruck.MainDriver?.DeviceToken ?? string.Empty);

            await pushNotificationService.SendNotificationAsync(
                "Trip reassigned",
                $"Trip #{trip.Number} '{trip.Name}' has been reassigned to another truck",
                oldTruck.MainDriver?.DeviceToken ?? string.Empty);

            // Send in-app notification for TMS portal users
            var newDriverName = newTruck.MainDriver?.GetFullName() ?? newTruck.Number;
            await notificationService.SendNotificationAsync(
                "Trip reassigned",
                $"Trip #{trip.Number} has been reassigned to {newDriverName}");
        }
    }

    private int RemoveLoads(Trip trip, Dictionary<Guid, Load> loadsMap, IEnumerable<Guid>? loadIdsToRemove)
    {
        if (loadIdsToRemove is null)
        {
            return 0;
        }

        var loadRepo = uow.Repository<Load>();

        var before = loadsMap.Count;
        foreach (var loadId in loadIdsToRemove.Distinct())
        {
            if (loadsMap.Remove(loadId, out var load))
            {
                trip.RemoveLoad(loadId);
                loadRepo.Delete(load);
            }
        }

        return before - loadsMap.Count;
    }

    private async Task<Result<int>> AttachExistingLoadsAsync(
        Trip trip,
        Dictionary<Guid, Load> loadsMap,
        IEnumerable<Guid>? attachIds,
        CancellationToken ct)
    {
        var count = 0;
        if (attachIds is null)
        {
            return Result<int>.Ok(count);
        }

        var ids = attachIds.Distinct().Where(id => !loadsMap.ContainsKey(id)).ToArray();
        if (ids.Length == 0)
        {
            return Result<int>.Ok(count);
        }

        var toAttach = await uow.Repository<Load>().GetListAsync(l => ids.Contains(l.Id), ct);

        foreach (var load in toAttach)
        {
            if (load.Status != LoadStatus.Draft)
            {
                return Result<int>.Fail(
                    $"Only loads in 'Draft' status can be attached. Load '{load.Id}' is in '{load.Status}' status.");
            }

            load.AssignedTruckId = trip.TruckId;
            load.AssignedTruck = trip.Truck;

            // If the load already has trip stops, we need to remove them first from the previous trip
            // to avoid duplicates. This can happen if the load was previously attached to another trip.
            if (load.TripStops.Count > 0)
            {
                var previousTrip = await uow.Repository<Trip>().GetByIdAsync(load.TripStops.First().TripId, ct);
                previousTrip?.RemoveLoad(load.Id);
            }

            // Add the existing load to the new trip
            trip.AddLoads([load]);

            loadsMap[load.Id] = load;
            count++;
        }

        return Result<int>.Ok(count);
    }

    private async Task<int> CreateNewLoadsAsync(
        Trip trip,
        Dictionary<Guid, Load> loadsMap,
        IEnumerable<CreateTripLoadCommand>? newLoadCommands)
    {
        if (newLoadCommands is null)
        {
            return 0;
        }

        var createdCount = 0;
        var loadParametersList = new List<CreateLoadParameters>();

        foreach (var c in newLoadCommands)
        {
            var createLoadParameters = new CreateLoadParameters(
                c.Name,
                c.Type,
                (c.OriginAddress, c.OriginLocation),
                (c.DestinationAddress, c.DestinationLocation),
                c.DeliveryCost,
                c.Distance,
                c.CustomerId,
                trip.TruckId,
                c.AssignedDispatcherId,
                trip.Id);

            loadParametersList.Add(createLoadParameters);
            createdCount++;
        }

        var newLoads = await loadService.CreateLoadsAsync(loadParametersList);
        foreach (var load in newLoads)
        {
            loadsMap[load.Id] = load;
        }

        return createdCount;
    }

    /// <summary>
    ///     Updates the order of existing trip stops based on the optimized stops from the route optimizer.
    /// </summary>
    private void UpdateStopOrder(Trip trip, IEnumerable<TripStopDto> optimizedStops)
    {
        // Build a lookup for the new order: key = (LoadId, Type), value = new Order
        var orderLookup = optimizedStops.ToDictionary(
            s => (s.LoadId, s.Type),
            s => s.Order);

        // Update the order of existing stops
        foreach (var stop in trip.Stops)
        {
            var key = (stop.LoadId, stop.Type);
            if (orderLookup.TryGetValue(key, out var newOrder))
            {
                stop.Order = newOrder;
            }
            else
            {
                logger.LogWarning(
                    "Could not find optimized order for stop with LoadId '{LoadId}' and Type '{Type}'",
                    stop.LoadId, stop.Type);
            }
        }
    }
}
