using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class SetDriverDeviceTokenCommand : IRequest<ResponseResult>
{
    public string? UserId { get; set; }
    public string? DeviceToken { get; set; }
}
