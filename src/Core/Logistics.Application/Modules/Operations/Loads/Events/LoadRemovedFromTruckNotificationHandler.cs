using Logistics.Application.Modules.Operations.Notifications.Services;
using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;

namespace Logistics.Application.Modules.Operations.Loads.Events;

/// <summary>Tells the truck's drivers a load was taken off them: push and the driver chats.</summary>
internal sealed class LoadRemovedFromTruckNotificationHandler(IEventNotifier notifier)
    : INotificationHandler<LoadRemovedFromTruckEvent>
{
    public async Task Handle(LoadRemovedFromTruckEvent @event, CancellationToken cancellationToken)
    {
        var data = new Dictionary<string, string> { ["loadId"] = @event.LoadId.ToString() };

        await notifier.PushAsync("Load update", $"Load #{@event.LoadNumber} has been removed from you", data,
            @event.MainDriverDeviceToken, @event.SecondaryDriverDeviceToken);
        await notifier.PostToTelegramAsync("Load Removed", $"Load #{@event.LoadNumber} has been removed from truck",
            TelegramChatRole.Driver, cancellationToken);
    }
}
