using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class SetDriverDeviceTokenCommand : IAppRequest
{
    public Guid UserId { get; set; }
    public string? DeviceToken { get; set; }
}
