using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class SetDriverDeviceTokenCommand : IRequest<Result>
{
    public string UserId { get; set; } = null!;
    public string? DeviceToken { get; set; }
}
