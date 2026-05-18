using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.Integrations.UpdateNotifications.Commands;

public class UpdateNotificationCommand : ICommand
{
    public Guid Id { get; set; }
    public bool? IsRead { get; set; }
}
