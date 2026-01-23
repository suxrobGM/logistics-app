using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>
/// Represents a freight load to be transported.
/// </summary>
public partial class Load : AuditableEntity, ITenantEntity
{
    public long Number { get; private set; }
    public required string Name { get; set; }
    public required LoadType Type { get; set; }

    public LoadStatus Status { get; private set; } = LoadStatus.Draft;

    public required Address OriginAddress { get; set; }
    public required GeoPoint OriginLocation { get; set; }

    public required Address DestinationAddress { get; set; }
    public required GeoPoint DestinationLocation { get; set; }

    public required Money DeliveryCost { get; set; }

    /// <summary>
    /// Total distance of the load in kilometers.
    /// </summary>
    public double Distance { get; set; }

    public bool CanConfirmPickUp { get; set; }
    public bool CanConfirmDelivery { get; set; }

    public DateTime? DispatchedAt { get; private set; }
    public DateTime? PickedUpAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    public Guid CustomerId { get; set; }
    public virtual required Customer Customer { get; set; }

    public Guid? AssignedTruckId { get; set; }
    public virtual Truck? AssignedTruck { get; set; }

    public Guid? AssignedDispatcherId { get; set; }
    public virtual Employee? AssignedDispatcher { get; set; }

    public virtual List<LoadInvoice> Invoices { get; set; } = [];
    public virtual List<LoadDocument> Documents { get; set; } = [];
    public virtual ICollection<TripStop> TripStops { get; } = [];

    /// <summary>
    /// If the load was booked from a load board, the provider type.
    /// </summary>
    public LoadBoardProviderType? ExternalSourceProvider { get; set; }

    /// <summary>
    /// External listing ID from the load board provider.
    /// </summary>
    public string? ExternalSourceId { get; set; }

    /// <summary>
    /// Broker reference number from the load board.
    /// </summary>
    public string? ExternalBrokerReference { get; set; }

    public decimal CalcDriverShare()
    {
        return DeliveryCost * (decimal)(AssignedTruck?.GetDriversShareRatio() ?? 0);
    }
}
