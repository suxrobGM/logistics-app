using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>
/// Represents a trip consisting of multiple stops for picking up and delivering loads.
/// </summary>
public partial class Trip : AuditableEntity, ITenantEntity
{
    /// <summary>
    /// Sequential number of the trip, unique within the tenant.
    /// </summary>
    public long Number { get; private set; }

    public required string Name { get; set; }

    /// <summary>
    /// Total distance of the trip in kilometers.
    /// </summary>
    public double TotalDistance { get; set; }

    public DateTime? DispatchedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    public TripStatus Status { get; private set; } = TripStatus.Draft;

    public Guid? TruckId { get; set; }
    public virtual Truck? Truck { get; set; }

    public virtual List<TripStop> Stops { get; } = [];

    /// <summary>
    /// Gets the origin address (first stop).
    /// </summary>
    public Address GetOriginAddress()
    {
        return Stops.OrderBy(s => s.Order).First().Address;
    }

    /// <summary>
    /// Gets the destination address (last stop).
    /// </summary>
    public Address GetDestinationAddress()
    {
        return Stops.OrderBy(s => s.Order).Last().Address;
    }

    /// <summary>
    /// Calculates the total revenue from all loads in the trip.
    /// </summary>
    public decimal CalcTotalRevenue()
    {
        return Stops
            .Where(s => s.Type == TripStopType.DropOff)
            .Sum(s => s.Load.DeliveryCost.Amount);
    }

    /// <summary>
    /// Calculates the total driver share from all loads in the trip.
    /// </summary>
    public decimal CalcDriversShare()
    {
        return Stops
            .Where(s => s.Type == TripStopType.DropOff)
            .Sum(s => s.Load.CalcDriverShare());
    }
}
