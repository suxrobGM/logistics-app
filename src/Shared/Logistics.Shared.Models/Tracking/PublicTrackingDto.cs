using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

/// <summary>
///     Load tracking information for public (unauthenticated) access.
/// </summary>
public record PublicTrackingDto
{
    public long LoadNumber { get; set; }
    public string? LoadName { get; set; }
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

    // Driver info (limited)
    public string? DriverName { get; set; }
    public string? TruckNumber { get; set; }

    // Document info
    public int DocumentCount { get; set; }
    public bool HasProofOfDelivery { get; set; }
    public bool HasBillOfLading { get; set; }

    // Tenant branding
    public string TenantName { get; set; } = string.Empty;
    public string? TenantLogoUrl { get; set; }
}
