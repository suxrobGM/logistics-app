using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

/// <summary>Everything a new load is built from, whether created alone or inline by a trip.</summary>
public interface ILoadDetails : ILoadScheduleFields
{
    string Name { get; }
    LoadType Type { get; }
    Address OriginAddress { get; }
    GeoPoint OriginLocation { get; }
    Address DestinationAddress { get; }
    GeoPoint DestinationLocation { get; }
    decimal DeliveryCost { get; }

    /// <summary>Distance in metres.</summary>
    double Distance { get; }

    Guid AssignedDispatcherId { get; }
    Guid CustomerId { get; }
    LoadSource Source { get; }
    string? Notes { get; }
    Guid? ContainerId { get; }
    Guid? OriginTerminalId { get; }
    Guid? DestinationTerminalId { get; }
    HazmatClass? HazmatClass { get; }
    string? UnNumber { get; }
    List<LoadVehicleInput> Vehicles { get; }
    decimal FuelSurcharge { get; }
    List<LoadAccessorialDto> Accessorials { get; }
    FreightClass? FreightClass { get; }
    string? NmfcCode { get; }
    int? PalletCount { get; }
    int? PieceCount { get; }
    double? Weight { get; }
    string? Dimensions { get; }
    bool IsStackable { get; }
}
