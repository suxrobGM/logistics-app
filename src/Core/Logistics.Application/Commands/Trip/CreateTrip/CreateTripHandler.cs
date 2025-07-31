using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class CreateTripHandler : RequestHandler<CreateTripCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;
    private readonly ILoadService _loadService;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ILogger<CreateTripHandler> _logger;

    public CreateTripHandler(
        ITenantUnityOfWork tenantUow,
        ILoadService loadService,
        IPushNotificationService pushNotificationService,
        ILogger<CreateTripHandler> logger)
    {
        _tenantUow = tenantUow;
        _loadService = loadService;
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    protected override async Task<Result> HandleValidated(
        CreateTripCommand req, CancellationToken cancellationToken)
    {
        List<Load> loads = [];
        var truck = await _tenantUow.Repository<Truck>().GetByIdAsync(req.TruckId);

        if (truck is null)
        {
            return Result.Fail($"Could not find the truck with ID '{req.TruckId}'");
        }

        if (req.ExistingLoadIds is not null)
        {
            loads = await _tenantUow.Repository<Load>().GetListAsync(i => req.ExistingLoadIds.Contains(i.Id));
            _logger.LogInformation(
                "Found {Count} existing loads for trip '{TripName}' with truck '{TruckId}'",
                loads.Count, req.Name, req.TruckId);
        }

        if (req.NewLoads is not null)
        {
            var newLoadsCount = 0;
            foreach (var newLoad in req.NewLoads)
            {
                var createLoadParameters = new CreateLoadParameters(
                    newLoad.Name,
                    newLoad.Type,
                    (newLoad.OriginAddress.ToEntity(), newLoad.OriginAddressLong, newLoad.OriginAddressLat),
                    (newLoad.DestinationAddress.ToEntity(), newLoad.DestinationAddressLong, newLoad.DestinationAddressLat),
                    newLoad.DeliveryCost,
                    newLoad.Distance,
                    newLoad.Customer.Id,
                    newLoad.AssignedTruckId.Value,
                    newLoad.AssignedDispatcherId.Value);
                
                var newLoadEntity = await _loadService.CreateLoadAsync(createLoadParameters);
                loads.Add(newLoadEntity);
                newLoadsCount++;
            }
            
            _logger.LogInformation(
                "Created {Count} new loads for trip '{TripName}' with truck '{TruckId}'",
                newLoadsCount, req.Name, req.TruckId);
        }
        
        var trip = Trip.Create(req.Name, req.PlannedStart, truck, loads);

        await _tenantUow.Repository<Trip>().AddAsync(trip);
        await _tenantUow.SaveChangesAsync();
        _logger.LogInformation(
            "Created trip '{TripName}' with ID '{TripId}' for truck '{TruckId}'",
            trip.Name, trip.Id, req.TruckId);
        return Result.Succeed();
    }
}
