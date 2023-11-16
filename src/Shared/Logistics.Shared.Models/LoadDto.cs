using Logistics.Shared.Enums;

namespace Logistics.Shared.Models;

public class LoadDto
{
    public string Id { get; set; } = default!;
    public ulong RefId { get; set; }
    public string? Name { get; set; }
    public required AddressDto OriginAddress { get; set; }
    public double? OriginAddressLat { get; set; }
    public double? OriginAddressLong { get; set; }
    public required AddressDto DestinationAddress { get; set; }
    public double? DestinationAddressLat { get; set; }
    public double? DestinationAddressLong { get; set; }
    public decimal DeliveryCost { get; set; }
    public double Distance { get; set; }
    public DateTime DispatchedDate { get; set; }
    public DateTime? PickUpDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public bool CanConfirmPickUp { get; set; }
    public bool CanConfirmDelivery { get; set; }
    public LoadStatus Status { get; set; }
    public string? AssignedDispatcherId { get; set; }
    public string? AssignedDispatcherName { get; set; }
    public string? AssignedTruckId { get; set; }
    public string? AssignedTruckNumber { get; set; }
    public AddressDto? CurrentLocation { get; set; }
    public InvoiceDto? Invoice { get; set; }
    public CustomerDto? Customer { get; set; }
    public IEnumerable<string>? AssignedTruckDriversName { get; set; }
}
