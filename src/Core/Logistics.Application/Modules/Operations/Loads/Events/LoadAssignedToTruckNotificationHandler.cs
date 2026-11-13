using Logistics.Application.Modules.Operations.Notifications.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Events;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;

namespace Logistics.Application.Modules.Operations.Loads.Events;

/// <summary>Tells the truck's drivers about a new load: push, in-app and the driver chats.</summary>
internal sealed class LoadAssignedToTruckNotificationHandler(IEventNotifier notifier, ITenantUnitOfWork tenantUow)
    : INotificationHandler<LoadAssignedToTruckEvent>
{
    public async Task Handle(LoadAssignedToTruckEvent @event, CancellationToken cancellationToken)
    {
        // Read the number off the tracked load: the event is raised before the insert assigns it.
        var load = await tenantUow.Repository<Load>().GetByIdAsync(@event.LoadId, cancellationToken);
        var loadNumber = load?.Number;
        var data = new Dictionary<string, string> { ["loadId"] = @event.LoadId.ToString() };
        var assigned = $"Load #{loadNumber} has been assigned to {@event.DriverDisplayName}";

        await notifier.PushAsync("Received a load", $"A new load #{loadNumber} has been assigned to you", data,
            @event.MainDriverDeviceToken, @event.SecondaryDriverDeviceToken);
        await notifier.ShowInAppAsync("Load assigned", assigned);
        await notifier.PostToTelegramAsync("Load Assigned", assigned, TelegramChatRole.Driver, cancellationToken);
    }
}
