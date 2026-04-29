using System.ComponentModel.DataAnnotations.Schema;
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

    /// <summary>
    /// True when the assigned truck is inside the geofence of the next confirmation
    /// checkpoint (pickup if <see cref="Status"/> is <see cref="LoadStatus.Dispatched"/>,
    /// delivery if <see cref="LoadStatus.PickedUp"/>). Toggled by the proximity tracker.
    /// </summary>
    public bool IsInProximity { get; set; }

    /// <summary>
    /// Computed: true when the driver can confirm pickup right now.
    /// </summary>
    [NotMapped]
    public bool CanConfirmPickUp => Status == LoadStatus.Dispatched && IsInProximity;

    /// <summary>
    /// Computed: true when the driver can confirm delivery right now.
    /// </summary>
    [NotMapped]
    public bool CanConfirmDelivery => Status == LoadStatus.PickedUp && IsInProximity;

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

    public virtual LoadInvoice? Invoice { get; set; }

    public virtual List<LoadDocument> Documents { get; set; } = [];
    public virtual List<LoadException> Exceptions { get; set; } = [];
    public virtual ICollection<TripStop> TripStops { get; } = [];

    /// <summary>
    /// How this load entered the system (manual entry, email/PDF import, API, load board).
    /// </summary>
    public LoadSource Source { get; set; } = LoadSource.Manual;

    /// <summary>
    /// Customer-requested pickup window date.
    /// </summary>
    public DateTime? RequestedPickupDate { get; set; }

    /// <summary>
    /// Customer-requested delivery window date.
    /// </summary>
    public DateTime? RequestedDeliveryDate { get; set; }

    /// <summary>
    /// Free-form dispatcher / driver notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Optional intermodal container being moved by this load.
    /// </summary>
    public Guid? ContainerId { get; set; }

    public virtual Container? Container { get; set; }

    /// <summary>
    /// Optional origin terminal (port / rail / depot) — when set, the truck picks up here.
    /// <see cref="OriginAddress"/> is still populated as a denormalized snapshot.
    /// </summary>
    public Guid? OriginTerminalId { get; set; }

    public virtual Terminal? OriginTerminal { get; set; }

    /// <summary>
    /// Optional destination terminal — when set, the truck drops off here.
    /// </summary>
    public Guid? DestinationTerminalId { get; set; }

    public virtual Terminal? DestinationTerminal { get; set; }

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
