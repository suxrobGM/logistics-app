using Logistics.Domain.Core;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Domain.Primitives.Enums;

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
    public double? AddressLong { get; set; }
    public double? AddressLat { get; set; }
    
    public DateTime? Planned { get; set; }
    public DateTime? ArrivedAt { get; set; }
    
    public Guid LoadId { get; set; }
    
    /// <summary>
    /// The specific load handled at this stop.
    /// </summary>
    public virtual required Load Load { get; set; }
}
