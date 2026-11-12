using Logistics.Application.Modules.Platform.Notifications.Services;
using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;

namespace Logistics.Application.Modules.Operations.Loads.Events;

/// <summary>Prompts the truck's drivers to confirm a pickup or delivery once they are near the stop.</summary>
internal sealed class LoadProximityChangedNotificationHandler(IEventNotifier notifier)
    : INotificationHandler<LoadProximityChangedEvent>
{
    public async Task Handle(LoadProximityChangedEvent @event, CancellationToken cancellationToken)
    {
        var statusText = @event.StatusToConfirm switch
        {
            LoadStatus.PickedUp => "picked up",
            LoadStatus.Delivered => "delivered",
            LoadStatus.Dispatched => "dispatched",
            _ => @event.StatusToConfirm.ToString().ToLowerInvariant()
        };

        var data = new Dictionary<string, string> { ["loadId"] = @event.LoadId.ToString() };

        await notifier.PushAsync("Confirm load status",
            $"You can confirm the {statusText} date of load #{@event.LoadNumber}", data,
            @event.MainDriverDeviceToken, @event.SecondaryDriverDeviceToken);
    }
}
