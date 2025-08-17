using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class AddLoadToTripHandler : IAppRequestHandler<AddLoadToTripCommand, Result>
{
    private readonly ILoadService _loadService;
    private readonly ILogger<AddLoadToTripHandler> _logger;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ITenantUnitOfWork _tenantUow;

    public AddLoadToTripHandler(
        ITenantUnitOfWork tenantUow,
        ILoadService loadService,
        IPushNotificationService pushNotificationService,
        ILogger<AddLoadToTripHandler> logger)
    {
        _tenantUow = tenantUow;
        _loadService = loadService;
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    public async Task<Result> Handle(AddLoadToTripCommand req, CancellationToken ct)
    {
        var trip = await _tenantUow.Repository<Trip>().GetByIdAsync(req.TripId, ct);

        if (trip is null)
        {
            return Result.Fail($"Could not find the trip with ID '{req.TripId}'");
        }

        var existingLoad = await GetExistingLoadAsync(req.ExistingLoadId, trip);

        if (existingLoad is not null)
        {
            trip.UpdateTripLoads([existingLoad]);
        }

        var newLoad = await CreateNewLoad(req.NewLoad, trip);

        if (newLoad is not null)
        {
            trip.UpdateTripLoads([newLoad]);
        }

        await _tenantUow.SaveChangesAsync(ct);
        _logger.LogInformation(
            "Added load to trip '{TripName}' with ID '{TripId}' for truck '{TruckId}'",
            trip.Name, trip.Id, trip.TruckId);
        return Result.Ok();
    }

    private async Task<Load?> CreateNewLoad(CreateTripLoadCommand? newLoad, Trip trip)
    {
        if (newLoad is null)
        {
            return null;
        }

        var createLoadParameters = new CreateLoadParameters(
            newLoad.Name,
            newLoad.Type,
            (newLoad.OriginAddress, newLoad.OriginLocation),
            (newLoad.DestinationAddress, newLoad.DestinationLocation),
            newLoad.DeliveryCost,
            newLoad.Distance,
            newLoad.CustomerId,
            trip.TruckId,
            newLoad.AssignedDispatcherId);

        var newLoadEntity = await _loadService.CreateLoadAsync(createLoadParameters);

        _logger.LogInformation("Created a new load for trip '{TripName}' with truck '{TruckId}'", trip.Name,
            trip.TruckId);
        return newLoadEntity;
    }

    /// <summary>
    ///     Retrieves existing loads based on the provided command and assigns them to the specified truck.
    ///     Clears the trip stop to avoid conflicts with the new trip.
    /// </summary>
    private async Task<Load?> GetExistingLoadAsync(Guid? loadId, Trip trip)
    {
        if (loadId is null)
        {
            return null;
        }

        var load = await _tenantUow.Repository<Load>().GetByIdAsync(loadId.Value);

        if (load is null)
        {
            return null;
        }

        load.AssignedTruck = trip.Truck;
        load.AssignedTruckId = trip.TruckId;
        load.TripStop = null; // Clear the trip stop to avoid conflicts

        _logger.LogInformation("Added existing load '{LoadId}' to trip '{TripName}' with truck '{TruckId}'", loadId,
            trip.Name, trip.TruckId);
        return load;
    }
}
