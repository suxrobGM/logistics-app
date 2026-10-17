using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Notifications.Commands;

public sealed class MarkNotificationsReadCommand : ICommand<Result>
{
    public required List<Guid> Ids { get; set; }
}
