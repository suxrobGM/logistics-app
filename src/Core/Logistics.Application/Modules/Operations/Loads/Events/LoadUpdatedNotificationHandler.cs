using Logistics.Application.Modules.Platform.Notifications.Services;
using Logistics.Domain.Events;
using Logistics.Mediator;

namespace Logistics.Application.Modules.Operations.Loads.Events;

/// <summary>Tells the truck's drivers and every chat that a load's details changed.</summary>
internal sealed class LoadUpdatedNotificationHandler(IEventNotifier notifier)
    : INotificationHandler<LoadUpdatedEvent>
{
    public async Task Handle(LoadUpdatedEvent @event, CancellationToken cancellationToken)
    {
        var data = new Dictionary<string, string> { ["loadId"] = @event.LoadId.ToString() };

        await notifier.PushAsync("Load update", $"Load #{@event.LoadNumber} details have been updated. Check details.",
            data, @event.MainDriverDeviceToken, @event.SecondaryDriverDeviceToken);
        await notifier.PostToTelegramAsync("Load Updated", $"Load #{@event.LoadNumber} details have been updated",
            role: null, cancellationToken);
    }
}
