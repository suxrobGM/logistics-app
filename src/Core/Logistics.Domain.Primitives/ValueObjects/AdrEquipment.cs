using System.ComponentModel.DataAnnotations.Schema;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Primitives.ValueObjects;

/// <summary>
/// Truck-side ADR (Accord Dangereux Routier) equipment configuration. Tracks whether the
/// truck is certified to carry hazardous goods, which classes it is approved for, the
/// expiry of the certificate, and the orange-plate identification number required by ADR.
/// </summary>
[ComplexType]
public record AdrEquipment
{
    public bool IsAdrCertified { get; set; }
    public DateTime? AdrCertExpiresAt { get; set; }
    public HazmatClassFlags AllowedClasses { get; set; } = HazmatClassFlags.None;
    public string? OrangePlateNumber { get; set; }

    public static AdrEquipment None => new();

    public bool IsCertExpired(DateTime utcNow) =>
        IsAdrCertified && AdrCertExpiresAt.HasValue && AdrCertExpiresAt.Value <= utcNow;

    public bool AllowsClass(HazmatClass hazmatClass) =>
        IsAdrCertified && AllowedClasses.HasFlag(hazmatClass.ToFlag());
}
