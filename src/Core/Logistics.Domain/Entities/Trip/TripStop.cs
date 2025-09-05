using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

public class TripStop : Entity, ITenantEntity
{
    public Guid TripId { get; set; }
    public virtual required Trip Trip { get; set; }

    /// <summary>
    /// 1-based position in the overall route.
    /// </summary>
    public int Order { get; set; }
    public TripStopType Type { get; set; }

    public required Address Address { get; set; }
    public required GeoPoint Location { get; set; }

    public DateTime? ArrivedAt { get; set; }

    public Guid LoadId { get; set; }

    /// <summary>
    /// The specific load handled at this stop.
    /// </summary>
    public virtual required Load Load { get; set; }
}
