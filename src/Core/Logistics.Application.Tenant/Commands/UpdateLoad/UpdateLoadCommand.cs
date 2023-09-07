using Logistics.Domain.Enums;

namespace Logistics.Application.Tenant.Commands;

public class UpdateLoadCommand : Request<ResponseResult>
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? SourceAddress { get; set; }
    public string? DestinationAddress { get; set; }
    public double? DeliveryCost { get; set; }
    public double? Distance { get; set; }
    public string? AssignedDispatcherId { get; set; }
    public string? AssignedTruckId { get; set; }
    public LoadStatus? Status { get; set; }
}
