namespace Logistics.Application.Tenant.Commands;

public class UpdateLoadProximityCommand : Request<ResponseResult>
{
    public string? LoadId { get; set; }
    public bool? CanConfirmPickUp { get; set; }
    public bool? CanConfirmDelivery { get; set; }
}
