namespace Logistics.Application.Tenant.Commands;

public class CreateLoadCommand : Request<ResponseResult>
{
    public string? Name { get; set; }
    public string? OriginAddress { get; set; }
    public string? OriginCoordinates { get; set; }
    public string? DestinationAddress { get; set; }
    public string? DestinationCoordinates { get; set; }
    public double DeliveryCost { get; set; }
    public double Distance { get; set; }
    public string? AssignedDispatcherId { get; set; }
    public string? AssignedDriverId { get; set; }
}
