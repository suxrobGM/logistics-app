using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class SetDriverDeviceTokenCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
    public string? DeviceToken { get; set; }
}
