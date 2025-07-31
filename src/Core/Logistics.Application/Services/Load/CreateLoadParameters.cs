using Logistics.Domain.ValueObjects;
using Logistics.Shared.Consts;

namespace Logistics.Application.Services;

public record CreateLoadParameters(
    string Name,
    LoadType Type,
    (Address address, double? @long, double? lat) Origin,
    (Address address, double? @long, double? lat) Destination,
    decimal DeliveryCost,
    double Distance,
    Guid CustomerId,
    Guid TruckId, 
    Guid DispatcherId
);
