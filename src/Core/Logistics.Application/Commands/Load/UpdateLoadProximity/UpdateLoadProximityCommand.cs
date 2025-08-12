using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class UpdateLoadProximityCommand : IAppRequest
{
    public Guid LoadId { get; set; }
    public bool? CanConfirmPickUp { get; set; }
    public bool? CanConfirmDelivery { get; set; }
}
