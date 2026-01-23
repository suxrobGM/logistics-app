using Logistics.Application.Abstractions;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateLoadHandler(ITenantUnitOfWork tenantUow)
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
            var truckChanged = await AssignTruckIfUpdated(req, load);

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

            // Raise LoadUpdatedEvent for existing truck (if not changing truck)
            // Truck change events are handled by AssignToTruck method
            if (!truckChanged)
            {
                load.MarkUpdated();
            }

            // SaveChanges dispatches domain events:
            // - LoadAssignedToTruckEvent (if truck assigned)
            // - LoadRemovedFromTruckEvent (if truck changed, for old truck)
            // - LoadUpdatedEvent (if truck not changed but details updated)
            await tenantUow.SaveChangesAsync(ct);

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

    private async Task<bool> AssignTruckIfUpdated(UpdateLoadCommand req, Load loadEntity)
    {
        if (req.AssignedTruckId is null)
        {
            return false;
        }

        var truck = await tenantUow.Repository<Truck>().GetByIdAsync(req.AssignedTruckId.Value);
        if (truck is null)
        {
            throw new InvalidOperationException($"Could not find a truck with ID '{req.AssignedTruckId}'");
        }

        if (loadEntity.AssignedTruckId == truck.Id)
        {
            return false;
        }

        // Use domain method which raises LoadAssignedToTruckEvent for notifications
        loadEntity.AssignToTruck(truck);
        return true;
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
}
