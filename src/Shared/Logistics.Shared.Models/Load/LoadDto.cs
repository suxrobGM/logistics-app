using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

public class LoadDto
{
    public Guid Id { get; set; }
    public long Number { get; set; }
    public string? Name { get; set; }
    public required Address OriginAddress { get; set; }
    public required GeoPoint OriginLocation { get; set; }
    public required Address DestinationAddress { get; set; }
    public required GeoPoint DestinationLocation { get; set; }
    public decimal DeliveryCost { get; set; }
    public double Distance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public bool CanConfirmPickUp { get; set; }
    public bool CanConfirmDelivery { get; set; }
    public LoadStatus Status { get; set; }
    public LoadType Type { get; set; }
    public Guid? AssignedDispatcherId { get; set; }
    public string? AssignedDispatcherName { get; set; }
    public Guid? AssignedTruckId { get; set; }
    public string? AssignedTruckNumber { get; set; }
    public Address? CurrentAddress { get; set; }
    public GeoPoint? CurrentLocation { get; set; }
    public CustomerDto? Customer { get; set; }

    public Guid? TripId { get; set; }
    public long? TripNumber { get; set; }
    public string? TripName { get; set; }

    public IEnumerable<string>? AssignedTruckDriversName { get; set; }
    public IEnumerable<InvoiceDto> Invoices { get; set; } = [];
}
