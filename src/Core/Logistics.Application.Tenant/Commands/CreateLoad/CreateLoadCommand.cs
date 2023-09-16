namespace Logistics.Application.Tenant.Commands;

public class CreateLoadCommand : Request<ResponseResult>
{
    public string? Name { get; set; }
    public string? OriginAddress { get; set; }
    public double OriginAddressLat { get; set; }
    public double OriginAddressLong { get; set; }
    public string? DestinationAddress { get; set; }
    public double DestinationAddressLat { get; set; }
    public double DestinationAddressLong { get; set; }
    public double DeliveryCost { get; set; }
    public double Distance { get; set; }
    public string? AssignedDispatcherId { get; set; }
    public string? AssignedTruckId { get; set; }
}
