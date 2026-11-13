using Logistics.Domain.Core;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>Minimum rate the negotiation agent accepts on a lane, by origin and destination country and state. A null state matches any state in that country; the most specific matching row wins.</summary>
public class LaneRateFloor : AuditableEntity, ITenantEntity
{
    public string OriginCountry { get; set; } = "US";

    /// <summary>Null means any state in <see cref="OriginCountry"/>.</summary>
    public string? OriginState { get; set; }

    public string DestinationCountry { get; set; } = "US";

    /// <summary>Null means any state in <see cref="DestinationCountry"/>.</summary>
    public string? DestinationState { get; set; }

    public required decimal MinRatePerKm { get; set; }

    public decimal? MinTotalRate { get; set; }

    public string? Notes { get; set; }
}
