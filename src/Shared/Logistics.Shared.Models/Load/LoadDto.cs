using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

public class LoadDto
{
    public Guid Id { get; set; }
    public long Number { get; set; }
    public string? Name { get; set; }
    public required Address OriginAddress { get; set; }
    public required GeoPoint OriginLocation { get; set; }
    public required Address DestinationAddress { get; set; }
    public required GeoPoint DestinationLocation { get; set; }
    public decimal DeliveryCost { get; set; }
    public double Distance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public bool CanConfirmPickUp { get; set; }
    public bool CanConfirmDelivery { get; set; }
    public bool IsInProximity { get; set; }
    public LoadStatus Status { get; set; }
    public LoadType Type { get; set; }
    public LoadSource Source { get; set; }
    public DateTime? RequestedPickupDate { get; set; }
    public DateTime? RequestedDeliveryDate { get; set; }
    public string? Notes { get; set; }
    public bool IsHazmat { get; set; }
    public HazmatClass? HazmatClass { get; set; }
    public string? UnNumber { get; set; }
    public FreightClass? FreightClass { get; set; }
    public string? NmfcCode { get; set; }
    public int? PalletCount { get; set; }
    public int? PieceCount { get; set; }
    public double? Weight { get; set; }
    public string? Dimensions { get; set; }
    public bool IsStackable { get; set; }
    public decimal FuelSurcharge { get; set; }

    /// <summary>
    ///     Filled on a single-load read only.
    /// </summary>
    public List<LoadAccessorialDto> Accessorials { get; set; } = [];
    public Guid? ContainerId { get; set; }
    public string? ContainerNumber { get; set; }
    public ContainerIsoType? ContainerIsoType { get; set; }
    public DateOnly? ContainerLastFreeDay { get; set; }
    public bool? ContainerIsOnHold { get; set; }
    public Guid? OriginTerminalId { get; set; }
    public string? OriginTerminalName { get; set; }
    public string? OriginTerminalCode { get; set; }
    public Guid? DestinationTerminalId { get; set; }
    public string? DestinationTerminalName { get; set; }
    public string? DestinationTerminalCode { get; set; }
    public Guid? AssignedDispatcherId { get; set; }
    public string? AssignedDispatcherName { get; set; }
    public Guid? AssignedTruckId { get; set; }
    public string? AssignedTruckNumber { get; set; }
    public Address? CurrentAddress { get; set; }
    public GeoPoint? CurrentLocation { get; set; }
    public CustomerDto? Customer { get; set; }

    public Guid? TripId { get; set; }
    public long? TripNumber { get; set; }
    public string? TripName { get; set; }

    /// <summary>
    ///     The trailer recorded on the load's trip. Set by the single-load query only.
    /// </summary>
    public Guid? TrailerId { get; set; }
    public string? TrailerNumber { get; set; }

    /// <summary>
    ///     Vehicles on a car hauler (<see cref="LoadType.Vehicle"/>) load; empty otherwise.
    /// </summary>
    public List<LoadVehicleDto> Vehicles { get; set; } = [];

    /// <summary>
    ///     Set on a single-load read when the assigned truck's open loads carry more vehicles than
    ///     its <c>VehicleCapacity</c>. A warning, not a block.
    /// </summary>
    public string? VehicleCapacityWarning { get; set; }

    public IEnumerable<string>? AssignedTruckDriversName { get; set; }
    public InvoiceDto? Invoice { get; set; }
}
