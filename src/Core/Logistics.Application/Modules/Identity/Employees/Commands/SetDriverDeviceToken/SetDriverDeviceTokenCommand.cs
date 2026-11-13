
namespace Logistics.Application.Modules.Identity.Employees.Commands;

public class SetDriverDeviceTokenCommand : ICommand
{
    public Guid UserId { get; set; }
    public string? DeviceToken { get; set; }
}
