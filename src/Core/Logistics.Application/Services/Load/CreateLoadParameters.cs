using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Services;

/// <summary>
///     Create load parameters.
/// </summary>
/// <param name="Name">Load name</param>
/// <param name="Type">Load type</param>
/// <param name="Origin">Load origin geo point and address</param>
/// <param name="Destination">Load destination geo point and address</param>
/// <param name="DeliveryCost">Delivery cost</param>
/// <param name="Distance">Distance in meters</param>
/// <param name="CustomerId">Customer ID</param>
/// <param name="TruckId">Optional Truck ID - load can be created without truck assignment</param>
/// <param name="DispatcherId">Dispatcher ID</param>
/// <param name="TripId">Optionally specify trip ID</param>
public record CreateLoadParameters(
    string Name,
    LoadType Type,
    (Address address, GeoPoint location) Origin,
    (Address address, GeoPoint location) Destination,
    decimal DeliveryCost,
    double Distance,
    Guid CustomerId,
    Guid? TruckId,
    Guid DispatcherId,
    Guid? TripId = null
);
