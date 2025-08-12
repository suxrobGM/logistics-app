using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Services;

public record CreateLoadParameters(
    string Name,
    LoadType Type,
    (Address address, GeoPoint location) Origin,
    (Address address, GeoPoint location) Destination,
    decimal DeliveryCost,
    double Distance,
    Guid CustomerId,
    Guid TruckId,
    Guid DispatcherId
);
