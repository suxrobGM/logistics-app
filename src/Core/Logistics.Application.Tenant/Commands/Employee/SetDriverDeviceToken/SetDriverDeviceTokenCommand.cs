using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class SetDriverDeviceTokenCommand : IRequest<Result>
{
    public string UserId { get; set; } = default!;
    public string? DeviceToken { get; set; }
}
