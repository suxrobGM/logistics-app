namespace Logistics.Domain.Entities;

/// <summary>
/// Factory methods for Trip entity.
/// </summary>
public partial class Trip
{
    /// <summary>
    /// Creates a new Trip with stops for the provided loads.
    /// Raises NewTripCreatedEvent and TripAssignedToTruckEvent if truck is assigned.
    /// </summary>
    public static Trip Create(
        string name,
        Truck? truck,
        IEnumerable<Load>? loads = null,
        IEnumerable<TripStop>? optimizedStops = null,
        double? optimizedTotalDistance = null)
    {
        return TripFactory.Create(name, truck, loads, optimizedStops, optimizedTotalDistance);
    }
}
