using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

public class TripStop : Entity, ITenantEntity
{
    public TripStopType Type { get; set; }
    public Guid TripId { get; set; }
    public virtual required Trip Trip { get; set; }

    /// <summary>
    ///     1-based position in the overall route.
    /// </summary>
    public int Position { get; set; }

    public required Address Address { get; set; }
    public required GeoPoint Location { get; set; }

    public DateTime? ArrivedAt { get; set; }
    public DateTime? DepartedAt { get; set; }

    /// <summary>
    ///     When the dispatcher was told the truck had overstayed its free time here; at most one alert per stop.
    /// </summary>
    public DateTime? DetentionAlertedAt { get; set; }

    public DateTime? EstimatedArrivalAt { get; set; }

    /// <summary>Set once, so a stop gets at most one late notice.</summary>
    public DateTime? LateNotifiedAt { get; set; }

    public Guid LoadId { get; set; }

    /// <summary>
    ///     The specific load handled at this stop.
    /// </summary>
    public virtual required Load Load { get; set; }

    /// <summary>Change in loads on board at this stop: +1 for a pickup, -1 for a drop-off.</summary>
    public int GetDemand()
    {
        return Type is TripStopType.PickUp ? 1 : -1;
    }

    public DateTime? GetAppointmentEnd()
    {
        return Type is TripStopType.PickUp ? Load.PickupAppointmentTo : Load.DeliveryAppointmentTo;
    }
}
