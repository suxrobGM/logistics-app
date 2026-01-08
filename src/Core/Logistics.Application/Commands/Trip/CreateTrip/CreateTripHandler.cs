using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class CreateTripHandler : IAppRequestHandler<CreateTripCommand, Result>
{
    private readonly ILoadService _loadService;
    private readonly ILogger<CreateTripHandler> _logger;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ITenantUnitOfWork _tenantUow;

    public CreateTripHandler(
        ITenantUnitOfWork tenantUow,
        ILoadService loadService,
        IPushNotificationService pushNotificationService,
        ILogger<CreateTripHandler> logger, ITripOptimizer tripOptimizer)
    {
        _tenantUow = tenantUow;
        _loadService = loadService;
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    public async Task<Result> Handle(CreateTripCommand req, CancellationToken ct)
    {
        var truck = await _tenantUow.Repository<Truck>().GetByIdAsync(req.TruckId, ct);
        List<TripStop>? stops = null;

        if (truck is null)
        {
            return Result.Fail($"Could not find the truck with ID '{req.TruckId}'");
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

        await _tenantUow.Repository<Trip>().AddAsync(trip, ct);
        await _tenantUow.SaveChangesAsync(ct);
        _logger.LogInformation(
            "Created trip '{TripName}' with ID '{TripId}' for truck '{TruckId}'",
            trip.Name, trip.Id, req.TruckId);
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

            var newLoadEntity = await _loadService.CreateLoadAsync(createLoadParameters, false);
            loads.Add(newLoadEntity);

            // Map temporary ID to actual database ID
            if (!string.IsNullOrEmpty(newLoad.TempId))
            {
                tempIdToLoadMap[newLoad.TempId] = newLoadEntity.Id;
            }

            newLoadsCount++;
        }

        _logger.LogInformation(
            "Created {Count} new loads for trip '{TripName}' with truck '{TruckId}'",
            newLoadsCount, command.Name, command.TruckId);
        return (loads, tempIdToLoadMap);
    }

    /// <summary>
    ///     Retrieves existing loads based on the provided command and assigns them to the specified truck.
    ///     Clears the trip stop to avoid conflicts with the new trip.
    /// </summary>
    private async Task<List<Load>> GetExistingLoadsAsync(CreateTripCommand command, Truck truck)
    {
        if (command.AttachedLoadIds is null || !command.AttachedLoadIds.Any())
        {
            return [];
        }

        var loads = await _tenantUow.Repository<Load>().GetListAsync(i => command.AttachedLoadIds.Contains(i.Id));

        foreach (var load in loads)
        {
            load.AssignedTruck = truck;
            load.AssignedTruckId = truck.Id;
        }

        _logger.LogInformation(
            "Retrieved {Count} existing loads for trip '{TripName}' with truck '{TruckId}'",
            loads.Count, command.Name, command.TruckId);
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
            if (tempIdToLoadMap.ContainsKey(stopLoadIdStr))
            {
                actualLoadId = tempIdToLoadMap[stopLoadIdStr];
            }

            if (!loadMap.TryGetValue(actualLoadId, out var load))
            {
                _logger.LogWarning("Load with ID '{LoadId}' not found for optimized stop", actualLoadId);
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
