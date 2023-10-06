using Logistics.Domain.Enums;
using Logistics.Models;

namespace Logistics.Application.Tenant.Commands;

public class ConfirmLoadStatusCommand : Request<ResponseResult>
{
    public string? LoadId { get; set; }
    public LoadStatus? LoadStatus { get; set; }
    public Func<string, NotificationDto, Task>? SendNotificationAsync { get; set; }
}
