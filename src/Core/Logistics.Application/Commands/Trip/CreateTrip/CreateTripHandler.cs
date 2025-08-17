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
        ILogger<CreateTripHandler> logger)
    {
        _tenantUow = tenantUow;
        _loadService = loadService;
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    public async Task<Result> Handle(CreateTripCommand req, CancellationToken ct)
    {
        var truck = await _tenantUow.Repository<Truck>().GetByIdAsync(req.TruckId, ct);

        if (truck is null)
        {
            return Result.Fail($"Could not find the truck with ID '{req.TruckId}'");
        }

        var existingLoads = await GetExistingLoadsAsync(req, truck);
        var newLoads = await CreateNewLoads(req);

        // List of all loads for the trip
        var loads = new List<Load>([.. existingLoads, .. newLoads]);

        var trip = Trip.Create(req.Name, req.PlannedStart, truck, loads);

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
    private async Task<IEnumerable<Load>> CreateNewLoads(CreateTripCommand command)
    {
        if (command.NewLoads is null || !command.NewLoads.Any())
        {
            return [];
        }

        var loads = new List<Load>();
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

            var newLoadEntity = await _loadService.CreateLoadAsync(createLoadParameters);
            loads.Add(newLoadEntity);
            newLoadsCount++;
        }

        _logger.LogInformation(
            "Created {Count} new loads for trip '{TripName}' with truck '{TruckId}'",
            newLoadsCount, command.Name, command.TruckId);
        return loads;
    }

    /// <summary>
    ///     Retrieves existing loads based on the provided command and assigns them to the specified truck.
    ///     Clears the trip stop to avoid conflicts with the new trip.
    /// </summary>
    private async Task<List<Load>> GetExistingLoadsAsync(CreateTripCommand command, Truck truck)
    {
        if (command.ExistingLoadIds is null || !command.ExistingLoadIds.Any())
        {
            return [];
        }

        var loads = await _tenantUow.Repository<Load>().GetListAsync(i => command.ExistingLoadIds.Contains(i.Id));

        foreach (var load in loads)
        {
            load.AssignedTruck = truck;
            load.AssignedTruckId = truck.Id;
            load.TripStop = null; // Clear the trip stop to avoid conflicts
        }

        _logger.LogInformation(
            "Retrieved {Count} existing loads for trip '{TripName}' with truck '{TruckId}'",
            loads.Count, command.Name, command.TruckId);
        return loads;
    }
}
