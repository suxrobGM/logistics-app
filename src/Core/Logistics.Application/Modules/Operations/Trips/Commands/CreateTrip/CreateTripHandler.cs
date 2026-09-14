using Logistics.Application.Modules.Operations.Loads.Services;
using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Operations.Trips.Commands;

internal sealed class CreateTripHandler(
    ITenantUnitOfWork tenantUow,
    ILoadService loadService,
    ILogger<CreateTripHandler> logger)
    : IAppRequestHandler<CreateTripCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateTripCommand req, CancellationToken ct)
    {
        Truck? truck = null;
        List<TripStop>? stops = null;

        // Only fetch truck if TruckId is provided
        if (req.TruckId.HasValue)
        {
            truck = await tenantUow.Repository<Truck>().GetByIdAsync(req.TruckId.Value, ct);
            if (truck is null)
            {
                return Result<Guid>.Fail($"Could not find the truck with ID '{req.TruckId}'");
            }
        }

        var existingLoads = await GetExistingLoadsAsync(req, truck);
        var created = await CreateNewLoadsAsync(req);
        if (!created.IsSuccess)
        {
            return Result<Guid>.Fail(created.Error!, created.ErrorCode!);
        }

        var (newLoads, tempIdToLoadMap) = created.Value;

        // List of all loads for the trip
        var loads = new List<Load>([.. existingLoads, .. newLoads]);

        // Convert optimized stops DTOs to domain entities if provided
        if (req.OptimizedStops != null && req.OptimizedStops.Any())
        {
            stops = ConvertOptimizedStopsToDomain(req.OptimizedStops, loads, tempIdToLoadMap);
        }

        var trip = Trip.Create(req.Name, truck, loads, stops, req.TotalDistance);

        await tenantUow.Repository<Trip>().AddAsync(trip, ct);

        // Trip.Create() raises domain events for notifications:
        // - NewTripCreatedEvent (always)
        // - TripAssignedToTruckEvent (if truck assigned)
        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Created trip '{TripName}' with ID '{TripId}' for truck '{TruckId}'",
            trip.Name, trip.Id, req.TruckId?.ToString() ?? "unassigned");
        return Result<Guid>.Ok(trip.Id);
    }

    /// <summary>
    ///     Creates new loads based on the provided command.
    /// </summary>
    private async Task<Result<(IReadOnlyList<Load> Loads, Dictionary<string, Guid> TempIdToLoadId)>>
        CreateNewLoadsAsync(CreateTripCommand command)
    {
        var newLoads = command.NewLoads?.ToList() ?? [];
        var tempIdToLoadId = new Dictionary<string, Guid>();
        if (newLoads.Count == 0)
        {
            return Result<(IReadOnlyList<Load>, Dictionary<string, Guid>)>.Ok(([], tempIdToLoadId));
        }

        var created = await loadService.CreateLoadsAsync(
            newLoads.Select(l => new CreateLoadParameters(
                l.Name,
                l.Type,
                (l.OriginAddress, l.OriginLocation),
                (l.DestinationAddress, l.DestinationLocation),
                l.DeliveryCost,
                l.Distance,
                l.CustomerId,
                command.TruckId,
                l.AssignedDispatcherId)),
            saveChanges: false);

        if (!created.IsSuccess)
        {
            return Result<(IReadOnlyList<Load>, Dictionary<string, Guid>)>.Fail(created.Error!, created.ErrorCode!);
        }

        var loads = created.Value!;
        for (var i = 0; i < newLoads.Count; i++)
        {
            if (!string.IsNullOrEmpty(newLoads[i].TempId))
            {
                tempIdToLoadId[newLoads[i].TempId] = loads[i].Id;
            }
        }

        logger.LogInformation(
            "Created {Count} new loads for trip '{TripName}' with truck '{TruckId}'",
            loads.Count, command.Name, command.TruckId?.ToString() ?? "unassigned");
        return Result<(IReadOnlyList<Load>, Dictionary<string, Guid>)>.Ok((loads, tempIdToLoadId));
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
