using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Domain event methods for Trip entity.
/// </summary>
public partial class Trip
{
    /// <summary>
    /// Assigns or reassigns this trip to a truck and raises the TripAssignedToTruckEvent.
    /// </summary>
    /// <param name="newTruck">The new truck to assign this trip to.</param>
    /// <exception cref="InvalidOperationException">Thrown if trip is not in Draft status.</exception>
    public void AssignToTruck(Truck newTruck)
    {
        if (!TripStatusMachine.CanModify(Status))
        {
            throw new InvalidOperationException("Cannot change truck assignment unless trip is Draft.");
        }

        var oldTruck = Truck;
        var oldTruckId = TruckId;

        TruckId = newTruck.Id;
        Truck = newTruck;

        TripFactory.RaiseTruckAssignedEvent(
            this,
            newTruck,
            oldTruckId,
            oldTruck?.MainDriver?.DeviceToken);
    }
}
