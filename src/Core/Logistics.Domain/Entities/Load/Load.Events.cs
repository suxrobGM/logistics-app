using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Domain event methods for Load entity.
/// </summary>
public partial class Load
{
    /// <summary>
    /// Assigns this load to a truck and raises the LoadAssignedToTruckEvent.
    /// If reassigning, also raises LoadRemovedFromTruckEvent for the old truck.
    /// </summary>
    public void AssignToTruck(Truck truck)
    {
        var oldTruck = AssignedTruck;

        AssignedTruckId = truck.Id;
        AssignedTruck = truck;

        if (oldTruck is not null && oldTruck.Id != truck.Id)
        {
            RaiseRemovedFromTruckEvent(oldTruck);
        }

        RaiseAssignedToTruckEvent(truck);
    }

    /// <summary>
    /// Marks this load as removed from the truck and raises LoadRemovedFromTruckEvent.
    /// </summary>
    public void MarkRemovedFromTruck()
    {
        if (AssignedTruck is null)
        {
            return;
        }

        RaiseRemovedFromTruckEvent(AssignedTruck);
    }

    /// <summary>
    /// Marks this load as updated and raises LoadUpdatedEvent.
    /// </summary>
    public void MarkUpdated()
    {
        if (AssignedTruck is null)
        {
            return;
        }

        DomainEvents.Add(new LoadUpdatedEvent(
            Id,
            Number,
            AssignedTruck.Id,
            AssignedTruck.Number,
            AssignedTruck.MainDriver?.DeviceToken,
            AssignedTruck.SecondaryDriver?.DeviceToken));
    }

    private void RaiseAssignedToTruckEvent(Truck truck)
    {
        var driverDisplayName = truck.MainDriver?.GetFullName() ?? truck.Number;

        DomainEvents.Add(new LoadAssignedToTruckEvent(
            Id,
            Number,
            truck.Id,
            truck.Number,
            truck.MainDriver?.DeviceToken,
            truck.SecondaryDriver?.DeviceToken,
            driverDisplayName));
    }

    private void RaiseRemovedFromTruckEvent(Truck truck)
    {
        DomainEvents.Add(new LoadRemovedFromTruckEvent(
            Id,
            Number,
            truck.Id,
            truck.Number,
            truck.MainDriver?.DeviceToken,
            truck.SecondaryDriver?.DeviceToken));
    }

    private void RaiseProximityChangedEvent(LoadStatus statusToConfirm)
    {
        if (AssignedTruck is null)
        {
            return;
        }

        DomainEvents.Add(new LoadProximityChangedEvent(
            Id,
            Number,
            statusToConfirm,
            AssignedTruck.Id,
            AssignedTruck.Number,
            AssignedTruck.MainDriver?.DeviceToken,
            AssignedTruck.SecondaryDriver?.DeviceToken));
    }
}
