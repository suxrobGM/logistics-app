using Logistics.Application.Modules.Operations.Loads.Commands;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trips.Commands;

/// <summary>
///     Nested load payload for trip create/update; not a dispatched command. Carries the same fields as
///     <see cref="CreateLoadCommand"/> except the truck, which the trip sets.
/// </summary>
public record CreateTripLoadCommand : ILoadChargeFields
{
    /// <summary>Client-side id that optimized stops use to reference this load before it exists.</summary>
    public string? TempId { get; set; }

    public required string Name { get; set; }
    public LoadType Type { get; set; }

    public required Address OriginAddress { get; set; }
    public required GeoPoint OriginLocation { get; set; }

    public required Address DestinationAddress { get; set; }
    public required GeoPoint DestinationLocation { get; set; }

    public decimal DeliveryCost { get; set; }
    public double Distance { get; set; }
    public required Guid AssignedDispatcherId { get; set; }
    public required Guid CustomerId { get; set; }

    public LoadSource Source { get; set; } = LoadSource.Manual;
    public DateTime? RequestedPickupDate { get; set; }
    public DateTime? RequestedDeliveryDate { get; set; }
    public string? Notes { get; set; }
    public Guid? ContainerId { get; set; }
    public Guid? OriginTerminalId { get; set; }
    public Guid? DestinationTerminalId { get; set; }

    public bool IsHazmat { get; set; }
    public HazmatClass? HazmatClass { get; set; }
    public string? UnNumber { get; set; }

    /// <summary>Vehicles carried by a <see cref="LoadType.Vehicle"/> load.</summary>
    public List<LoadVehicleInput> Vehicles { get; set; } = [];

    public decimal FuelSurcharge { get; set; }
    decimal? ILoadChargeFields.FuelSurcharge => FuelSurcharge;
    public List<LoadAccessorialDto> Accessorials { get; set; } = [];

    public FreightClass? FreightClass { get; set; }
    public string? NmfcCode { get; set; }
    public int? PalletCount { get; set; }
    public int? PieceCount { get; set; }
    public double? Weight { get; set; }
    public string? Dimensions { get; set; }
    public bool IsStackable { get; set; }
}
