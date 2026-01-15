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

    // Document counts
    public int DocumentCount { get; set; }
    public bool HasProofOfDelivery { get; set; }
    public bool HasBillOfLading { get; set; }
}
