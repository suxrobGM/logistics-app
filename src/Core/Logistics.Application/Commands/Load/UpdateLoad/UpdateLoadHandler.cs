using Logistics.Application.Abstractions;
using Logistics.Application.Extensions;
using Logistics.Application.Services;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateLoadHandler(
    ITenantUnitOfWork tenantUow,
    IPushNotificationService pushNotificationService,
    INotificationService notificationService)
    : IAppRequestHandler<UpdateLoadCommand, Result>
{
    public async Task<Result> Handle(UpdateLoadCommand req, CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.Id, ct);

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

            var changes = await tenantUow.SaveChangesAsync(ct);

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

        var customer = await tenantUow.Repository<Customer>().GetByIdAsync(req.CustomerId.Value);
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

        var truck = await tenantUow.Repository<Truck>().GetByIdAsync(req.AssignedTruckId.Value);
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

        var dispatcher = await tenantUow.Repository<Employee>().GetByIdAsync(req.AssignedDispatcherId.Value);
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
        if (newTruck != null && oldTruck == null)
        {
            // First-time truck assignment - load was created without truck and now assigned
            await pushNotificationService.SendNewLoadNotificationAsync(loadEntity, newTruck);

            // Send in-app notification for TMS portal users
            var driverName = newTruck.MainDriver?.GetFullName() ?? newTruck.Number;
            await notificationService.SendNotificationAsync(
                "Load assigned to truck",
                $"Load #{loadEntity.Number} has been assigned to {driverName}");
            return;
        }

        if (oldTruck != null)
        {
            // Send updates to the old truck
            await pushNotificationService.SendUpdatedLoadNotificationAsync(loadEntity, oldTruck);
        }

        if (newTruck != null && oldTruck != null && oldTruck.Id != newTruck.Id)
        {
            // The truck was switched
            await pushNotificationService.SendNewLoadNotificationAsync(loadEntity, newTruck);
            await pushNotificationService.SendRemovedLoadNotificationAsync(loadEntity, oldTruck);

            // Send in-app notification for TMS portal users
            var newDriverName = newTruck.MainDriver?.GetFullName() ?? newTruck.Number;
            await notificationService.SendNotificationAsync(
                "Load assigned",
                $"Load #{loadEntity.Number} has been reassigned to {newDriverName}");
        }
    }
}
