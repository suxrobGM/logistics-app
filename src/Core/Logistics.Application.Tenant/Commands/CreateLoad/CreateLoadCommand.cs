using System.ComponentModel.DataAnnotations;
using Logistics.Domain.Constraints;

namespace Logistics.Application.Tenant.Commands;

public class CreateLoadCommand : Request<ResponseResult>
{
    public string? Name { get; set; }
    public string? SourceAddress { get; set; }
    public string? DestinationAddress { get; set; }
    public double DeliveryCost { get; set; }
    public double Distance { get; set; }
    public string? AssignedDispatcherId { get; set; }
    public string? AssignedDriverId { get; set; }
}
