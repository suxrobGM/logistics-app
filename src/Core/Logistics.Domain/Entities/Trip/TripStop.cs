using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>
///     Represents a stop on a trip.
///     A stop can be a pickup or a drop-off location.
///     A trip can have multiple stops.
/// </summary>
public class TripStop : Entity, ITenantEntity
{
    public TripStopType Type { get; set; }
    public Guid TripId { get; set; }
    public virtual required Trip Trip { get; set; }

    /// <summary>
    ///     1-based position in the overall route.
    /// </summary>
    public int Order { get; set; }

    public required Address Address { get; set; }
    public required GeoPoint Location { get; set; }

    public DateTime? ArrivedAt { get; set; }

    public Guid LoadId { get; set; }

    /// <summary>
    ///     The specific load handled at this stop.
    /// </summary>
    public virtual required Load Load { get; set; }

    /// <summary>
    ///     Get demand for this stop.
    ///     +1 for pickup, -1 for drop-off.
    /// </summary>
    public int GetDemand()
    {
        return Type is TripStopType.PickUp ? 1 : -1;
    }
}
