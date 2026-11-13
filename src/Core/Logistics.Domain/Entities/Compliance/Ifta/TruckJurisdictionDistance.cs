using Logistics.Domain.Core;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>Distance a truck drove in one tax jurisdiction on one UTC date. Upserted as GPS positions arrive; quarterly IFTA reports sum these rows.</summary>
public class TruckJurisdictionDistance : Entity, ITenantEntity
{
    public required Guid TruckId { get; set; }
    public virtual Truck Truck { get; set; } = null!;

    public required TaxJurisdiction Jurisdiction { get; set; }

    /// <summary>UTC date the distance was driven.</summary>
    public required DateOnly Date { get; set; }

    /// <summary>Accumulated distance in meters.</summary>
    public decimal Meters { get; set; }
}
