using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>A recorded truck GPS position, kept 4 years (the IFTA audit window). Not an auditable entity, to keep rows small at about 288 per truck per day.</summary>
public class TruckLocationHistory : Entity, ITenantEntity
{
    public required Guid TruckId { get; set; }
    public virtual Truck Truck { get; set; } = null!;

    public required GeoPoint Location { get; set; }

    /// <summary>When the position was recorded (UTC).</summary>
    public required DateTime RecordedAt { get; set; }

    /// <summary>ELD provider the ping came from; null for driver-app/manual sources.</summary>
    public EldProviderType? Source { get; set; }

    public long? OdometerMeters { get; set; }
}
