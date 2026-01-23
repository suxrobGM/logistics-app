using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class CreateTripHandler(
    ITenantUnitOfWork tenantUow,
    ILoadService loadService,
    IPushNotificationService pushNotificationService,
    INotificationService notificationService,
    ILogger<CreateTripHandler> logger)
    : IAppRequestHandler<CreateTripCommand, Result>
{
    public async Task<Result> Handle(CreateTripCommand req, CancellationToken ct)
    {
        Truck? truck = null;
        List<TripStop>? stops = null;

        // Only fetch truck if TruckId is provided
        if (req.TruckId.HasValue)
        {
            truck = await tenantUow.Repository<Truck>().GetByIdAsync(req.TruckId.Value, ct);
            if (truck is null)
            {
                return Result.Fail($"Could not find the truck with ID '{req.TruckId}'");
            }
        }

        var existingLoads = await GetExistingLoadsAsync(req, truck);
        var (newLoads, tempIdToLoadMap) = await CreateNewLoads(req);

        // List of all loads for the trip
        var loads = new List<Load>([..existingLoads, ..newLoads]);

        // Convert optimized stops DTOs to domain entities if provided
        if (req.OptimizedStops != null && req.OptimizedStops.Any())
        {
            stops = ConvertOptimizedStopsToDomain(req.OptimizedStops, loads, tempIdToLoadMap);
        }

        var trip = Trip.Create(req.Name, truck, loads, stops, req.TotalDistance);

        await tenantUow.Repository<Trip>().AddAsync(trip, ct);
        await tenantUow.SaveChangesAsync(ct);

        // Send notifications if truck is assigned
        if (truck is not null)
        {
            // Send push notification to driver
            if (!string.IsNullOrEmpty(truck.MainDriver?.DeviceToken))
            {
                await pushNotificationService.SendNotificationAsync(
                    "New trip assigned",
                    $"Trip #{trip.Number} '{trip.Name}' has been assigned to you",
                    truck.MainDriver.DeviceToken);
            }

            // Send in-app notification for TMS portal users
            var driverName = truck.MainDriver?.GetFullName() ?? truck.Number;
            await notificationService.SendNotificationAsync(
                "New trip created",
                $"Trip #{trip.Number} has been created and assigned to {driverName}");
        }

        logger.LogInformation(
            "Created trip '{TripName}' with ID '{TripId}' for truck '{TruckId}'",
            trip.Name, trip.Id, req.TruckId?.ToString() ?? "unassigned");
        return Result.Ok();
    }

    /// <summary>
    ///     Creates new loads based on the provided command.
    /// </summary>
    private async Task<(IEnumerable<Load>, Dictionary<string, Guid>)> CreateNewLoads(CreateTripCommand command)
    {
        if (command.NewLoads is null || !command.NewLoads.Any())
        {
            return ([], new Dictionary<string, Guid>());
        }

        var loads = new List<Load>();
        var tempIdToLoadMap = new Dictionary<string, Guid>();
        var newLoadsCount = 0;

        foreach (var newLoad in command.NewLoads)
        {
            var createLoadParameters = new CreateLoadParameters(
                newLoad.Name,
                newLoad.Type,
                (newLoad.OriginAddress, newLoad.OriginLocation),
                (newLoad.DestinationAddress, newLoad.DestinationLocation),
                newLoad.DeliveryCost,
                newLoad.Distance,
                newLoad.CustomerId,
                command.TruckId,
                newLoad.AssignedDispatcherId);

            var newLoadEntity = await loadService.CreateLoadAsync(createLoadParameters, false);
            loads.Add(newLoadEntity);

            // Map temporary ID to actual database ID
            if (!string.IsNullOrEmpty(newLoad.TempId))
            {
                tempIdToLoadMap[newLoad.TempId] = newLoadEntity.Id;
            }

            newLoadsCount++;
        }

        logger.LogInformation(
            "Created {Count} new loads for trip '{TripName}' with truck '{TruckId}'",
            newLoadsCount, command.Name, command.TruckId?.ToString() ?? "unassigned");
        return (loads, tempIdToLoadMap);
    }

    /// <summary>
    ///     Retrieves existing loads based on the provided command and assigns them to the specified truck.
    ///     Clears the trip stop to avoid conflicts with the new trip.
    /// </summary>
    private async Task<List<Load>> GetExistingLoadsAsync(CreateTripCommand command, Truck? truck)
    {
        if (command.AttachedLoadIds is null || !command.AttachedLoadIds.Any())
        {
            return [];
        }

        var loads = await tenantUow.Repository<Load>().GetListAsync(i => command.AttachedLoadIds.Contains(i.Id));

        // Only assign truck to loads if truck is provided
        if (truck is not null)
        {
            foreach (var load in loads)
            {
                load.AssignedTruck = truck;
                load.AssignedTruckId = truck.Id;
            }
        }

        logger.LogInformation(
            "Retrieved {Count} existing loads for trip '{TripName}' with truck '{TruckId}'",
            loads.Count, command.Name, command.TruckId?.ToString() ?? "unassigned");
        return loads;
    }

    /// <summary>
    ///     Converts optimized stop DTOs to domain TripStop entities.
    /// </summary>
    private List<TripStop> ConvertOptimizedStopsToDomain(
        IEnumerable<TripStopDto> optimizedStops,
        List<Load> loads,
        Dictionary<string, Guid> tempIdToLoadMap)
    {
        var loadMap = loads.ToDictionary(l => l.Id);
        var tripStops = new List<TripStop>();

        foreach (var stopDto in optimizedStops)
        {
            var actualLoadId = stopDto.LoadId;

            // If the stop references a temporary ID, map it to the actual database ID
            var stopLoadIdStr = stopDto.LoadId.ToString();
            if (tempIdToLoadMap.TryGetValue(stopLoadIdStr, out var value))
            {
                actualLoadId = value;
            }

            if (!loadMap.TryGetValue(actualLoadId, out var load))
            {
                logger.LogWarning("Load with ID '{LoadId}' not found for optimized stop", actualLoadId);
                continue;
            }

            var tripStop = new TripStop
            {
                // Don't use the ID from DTO to avoid conflicts with existing entities
                Order = stopDto.Order,
                Type = stopDto.Type,
                Address = stopDto.Address,
                Location = stopDto.Location,
                Load = load,
                LoadId = load.Id,
                Trip = null! // Will be set when added to the trip
            };

            tripStops.Add(tripStop);
        }

        return tripStops;
    }
}
