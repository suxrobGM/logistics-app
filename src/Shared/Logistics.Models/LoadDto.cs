namespace Logistics.Models;

public class LoadDto
{
    public string? Id { get; set; }
    public ulong RefId { get; set; } = 100_000;
    public string? Name { get; set; }
    public string? OriginAddress { get; set; }
    public double? OriginAddressLat { get; set; }
    public double? OriginAddressLong { get; set; }
    public string? DestinationAddress { get; set; }
    public double? DestinationAddressLat { get; set; }
    public double? DestinationAddressLong { get; set; }
    public decimal DeliveryCost { get; set; }
    public double Distance { get; set; }
    public DateTime DispatchedDate { get; set; }
    public DateTime? PickUpDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public LoadStatusDto Status { get; set; }
    public string? AssignedDispatcherId { get; set; }
    public string? AssignedDispatcherName { get; set; }
    public TruckDto? AssignedTruck { get; set; }
}
