namespace Logistics.Application.Tenant.Commands;

public class SetDriverDeviceTokenCommand : Request<ResponseResult>
{
    public string? UserId { get; set; }
    public string? DeviceToken { get; set; }
}
