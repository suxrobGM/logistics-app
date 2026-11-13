using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Notifications.Commands;

public sealed class MarkNotificationsReadCommand : ICommand<Result>
{
    public required List<Guid> Ids { get; set; }
}
