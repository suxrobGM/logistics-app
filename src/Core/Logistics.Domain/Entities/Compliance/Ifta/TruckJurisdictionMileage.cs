using Logistics.Domain.Core;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>
/// Compact per-truck / per-jurisdiction / per-day mileage rollup, upserted incrementally as
/// GPS pings arrive. Quarterly IFTA reports aggregate these rows; the raw evidence lives in
/// <see cref="TruckLocationHistory"/>.
/// </summary>
public class TruckJurisdictionMileage : Entity, ITenantEntity
{
    public required Guid TruckId { get; set; }
    public virtual Truck Truck { get; set; } = null!;

    public required TaxJurisdiction Jurisdiction { get; set; }

    /// <summary>UTC date the miles were driven.</summary>
    public required DateOnly Date { get; set; }

    /// <summary>Accumulated distance in miles.</summary>
    public decimal Miles { get; set; }
}
