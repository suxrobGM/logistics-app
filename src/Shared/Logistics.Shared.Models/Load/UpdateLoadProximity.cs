namespace Logistics.Shared.Models;

public class UpdateLoadProximity
{
    public string? LoadId { get; set; }
    public bool? CanConfirmPickUp { get; set; }
    public bool? CanConfirmDelivery { get; set; }
}
