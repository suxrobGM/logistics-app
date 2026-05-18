using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.IdentityAccess.Employees.Commands;

public class SetDriverDeviceTokenCommand : ICommand
{
    public Guid UserId { get; set; }
    public string? DeviceToken { get; set; }
}
