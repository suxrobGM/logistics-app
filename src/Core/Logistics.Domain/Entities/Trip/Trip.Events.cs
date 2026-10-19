using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Domain event methods for Trip entity.
/// </summary>
public partial class Trip
{
    /// <summary>
    /// Assigns or reassigns this trip to a truck and raises the TripAssignedToTruckEvent. The trip
    /// takes the truck's current trailer, and its drivers once the trip is dispatched.
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
        TrailerId = newTruck.CurrentTrailerId;

        if (Status != TripStatus.Draft)
        {
            RecordDrivers();
        }

        TripFactory.RaiseTruckAssignedEvent(
            this,
            newTruck,
            oldTruckId,
            oldTruck?.MainDriver?.DeviceToken);
    }

    /// <summary>
    /// Pulls <paramref name="trailer"/> on this trip instead of the truck's current trailer.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the trip is past Dispatched or the trailer is not Active.
    /// </exception>
    public void AssignTrailer(Trailer trailer)
    {
        if (!TripStatusMachine.CanModify(Status))
        {
            throw new InvalidOperationException("Cannot change trailer assignment unless trip is Draft or Dispatched.");
        }

        trailer.EnsureCanBeHooked();
        TrailerId = trailer.Id;
        Trailer = trailer;
    }
}
