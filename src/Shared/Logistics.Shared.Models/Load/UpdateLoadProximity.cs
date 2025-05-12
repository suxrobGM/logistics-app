namespace Logistics.Shared.Models;

public record UpdateLoadProximity
{
    public Guid? LoadId { get; set; }
    public bool? CanConfirmPickUp { get; set; }
    public bool? CanConfirmDelivery { get; set; }
}
