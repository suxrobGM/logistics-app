using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

/// <summary>
/// Load information visible to customers in the portal.
/// </summary>
public class PortalLoadDto
{
    public Guid Id { get; set; }
    public long Number { get; set; }
    public string Name { get; set; } = string.Empty;
    public LoadStatus Status { get; set; }
    public LoadType Type { get; set; }

    /// <summary>
    /// Vehicles on a car hauler load; the portal offers the electronic BOL when this is not empty.
    /// </summary>
    public List<LoadVehicleDto> Vehicles { get; set; } = [];

    // Addresses
    public Address? OriginAddress { get; set; }
    public Address? DestinationAddress { get; set; }

    // Current truck location (for tracking)
    public Address? CurrentAddress { get; set; }
    public GeoPoint? CurrentLocation { get; set; }

    // Timestamps
    public DateTime? DispatchedAt { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Cost
    public decimal? DeliveryCost { get; set; }
    public double? Distance { get; set; }

    // Driver info (limited)
    public string? DriverName { get; set; }
    public string? TruckNumber { get; set; }

    // Schedule
    public DateTime? PickupAppointmentFrom { get; set; }
    public DateTime? PickupAppointmentTo { get; set; }
    public DateTime? DeliveryAppointmentFrom { get; set; }
    public DateTime? DeliveryAppointmentTo { get; set; }

    /// <summary>
    /// When the truck is expected at this load's next stop, from its live position. Null before dispatch
    /// or once delivered.
    /// </summary>
    public DateTime? EstimatedArrivalAt { get; set; }

    /// <summary>True once the next stop's ETA has run past its appointment window.</summary>
    public bool IsRunningLate { get; set; }

    /// <summary>The active public tracking link, when the carrier has shared one.</summary>
    public string? TrackingUrl { get; set; }

    /// <summary>Shipment update emails sent to the customer, oldest first.</summary>
    public List<PortalShipmentUpdateDto> Updates { get; set; } = [];

    // Document counts
    public int DocumentCount { get; set; }
    public bool HasProofOfDelivery { get; set; }
    public bool HasBillOfLading { get; set; }
}
