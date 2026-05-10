using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

/// <summary>
/// Wire shape for the truck's ADR (Accord Dangereux Routier) equipment configuration.
/// Mirrors the <c>AdrEquipment</c> value object but exposes <see cref="AllowedClasses"/>
/// as an array of <see cref="HazmatClass"/> instead of a flags bitfield, so OpenAPI can
/// describe it cleanly and clients don't need to know the bit values.
/// </summary>
public record AdrEquipmentDto
{
    public bool IsAdrCertified { get; set; }
    public DateTime? AdrCertExpiresAt { get; set; }
    public HazmatClass[] AllowedClasses { get; set; } = [];
    public string? OrangePlateNumber { get; set; }
}
