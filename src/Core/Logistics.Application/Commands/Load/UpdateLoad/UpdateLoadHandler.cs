using Logistics.Application.Abstractions;
using Logistics.Application.Extensions;
using Logistics.Application.Services;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateLoadHandler : IAppRequestHandler<UpdateLoadCommand, Result>
{
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ITenantUnitOfWork _tenantUow;

    public UpdateLoadHandler(
        ITenantUnitOfWork tenantUow,
        IPushNotificationService pushNotificationService)
    {
        _tenantUow = tenantUow;
        _pushNotificationService = pushNotificationService;
    }

    public async Task<Result> Handle(UpdateLoadCommand req, CancellationToken ct)
    {
        var load = await _tenantUow.Repository<Load>().GetByIdAsync(req.Id, ct);

        if (load is null)
        {
            return Result.Fail("Could not find the specified load");
        }

        try
        {
            var oldTruck = load.AssignedTruck;
            var newTruck = await AssignTruckIfUpdated(req, load);

            await AssignDispatcherIfUpdated(req, load);
            await UpdateCustomerIfUpdated(req, load);

            load.Name = PropertyUpdater.UpdateIfChanged(req.Name, load.Name);
            load.OriginAddress = PropertyUpdater.UpdateIfChanged(req.OriginAddress, load.OriginAddress);
            load.OriginLocation = PropertyUpdater.UpdateIfChanged(req.OriginLocation, load.OriginLocation);
            load.DestinationAddress = PropertyUpdater.UpdateIfChanged(req.DestinationAddress, load.DestinationAddress);
            load.DestinationLocation =
                PropertyUpdater.UpdateIfChanged(req.DestinationLocation, load.DestinationLocation);
            load.DeliveryCost = PropertyUpdater.UpdateIfChanged(req.DeliveryCost, load.DeliveryCost.Amount);
            load.Distance = PropertyUpdater.UpdateIfChanged(req.Distance, load.Distance);
            load.Type = PropertyUpdater.UpdateIfChanged(req.Type, load.Type);

            if (req.Status.HasValue)
            {
                load.UpdateStatus(req.Status.Value, true);
            }

            var changes = await _tenantUow.SaveChangesAsync(ct);

            if (changes > 0)
            {
                await NotifyTrucksAboutUpdates(oldTruck, newTruck, load);
            }

            return Result.Ok();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    private async Task UpdateCustomerIfUpdated(UpdateLoadCommand req, Load loadEntity)
    {
        if (req.CustomerId is null)
        {
            return;
        }

        var customer = await _tenantUow.Repository<Customer>().GetByIdAsync(req.CustomerId.Value);
        if (customer is null)
        {
            throw new InvalidOperationException($"Could not find a customer with ID '{req.CustomerId}'");
        }

        if (loadEntity.CustomerId != customer.Id)
        {
            loadEntity.Customer = customer;
        }
    }

    private async Task<Truck?> AssignTruckIfUpdated(UpdateLoadCommand req, Load loadEntity)
    {
        if (req.AssignedTruckId is null)
        {
            return null;
        }

        var truck = await _tenantUow.Repository<Truck>().GetByIdAsync(req.AssignedTruckId.Value);
        if (truck is null)
        {
            throw new InvalidOperationException($"Could not find a truck with ID '{req.AssignedTruckId}'");
        }

        if (loadEntity.AssignedTruckId == truck.Id)
        {
            return null;
        }

        loadEntity.AssignedTruck = truck;
        return truck;
    }

    private async Task AssignDispatcherIfUpdated(UpdateLoadCommand req, Load loadEntity)
    {
        if (req.AssignedDispatcherId is null)
        {
            return;
        }

        var dispatcher = await _tenantUow.Repository<Employee>().GetByIdAsync(req.AssignedDispatcherId.Value);
        if (dispatcher is null)
        {
            throw new InvalidOperationException($"Could not find a dispatcher with ID '{req.AssignedDispatcherId}'");
        }

        if (loadEntity.AssignedDispatcherId != dispatcher.Id)
        {
            loadEntity.AssignedDispatcher = dispatcher;
        }
    }

    private async Task NotifyTrucksAboutUpdates(Truck? oldTruck, Truck? newTruck, Load loadEntity)
    {
        if (oldTruck != null)
            // send updates to the old truck
        {
            await _pushNotificationService.SendUpdatedLoadNotificationAsync(loadEntity, oldTruck);
        }

        if (newTruck != null && oldTruck != null && oldTruck.Id != newTruck.Id)
        {
            // The truck was switched
            await _pushNotificationService.SendNewLoadNotificationAsync(loadEntity, newTruck);
            await _pushNotificationService.SendRemovedLoadNotificationAsync(loadEntity, oldTruck);
        }
    }
}
