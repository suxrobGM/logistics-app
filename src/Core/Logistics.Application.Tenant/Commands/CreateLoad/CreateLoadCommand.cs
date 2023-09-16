namespace Logistics.Application.Tenant.Commands;

public class CreateLoadCommand : Request<ResponseResult>
{
    public string? Name { get; set; }
    public string? OriginAddress { get; set; }
    public double OriginLatitude { get; set; }
    public double OriginLongitude { get; set; }
    public string? DestinationAddress { get; set; }
    public double DestinationLatitude { get; set; }
    public double DestinationLongitude { get; set; }
    public double DeliveryCost { get; set; }
    public double Distance { get; set; }
    public string? AssignedDispatcherId { get; set; }
    public string? AssignedTruckId { get; set; }
}
