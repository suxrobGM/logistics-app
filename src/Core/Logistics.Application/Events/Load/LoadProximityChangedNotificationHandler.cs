using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Events;

/// <summary>
/// Handles notifications when load proximity changes and driver can confirm status.
/// Sends push notification to all drivers on the truck.
/// </summary>
internal sealed class LoadProximityChangedNotificationHandler(
    IPushNotificationService pushNotificationService,
    ILogger<LoadProximityChangedNotificationHandler> logger)
    : IDomainEventHandler<LoadProximityChangedEvent>
{
    public async Task Handle(LoadProximityChangedEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Load {LoadId} proximity changed to confirm {Status}, sending notifications",
            @event.LoadId, @event.StatusToConfirm);

        var statusText = @event.StatusToConfirm switch
        {
            LoadStatus.PickedUp => "picked up",
            LoadStatus.Delivered => "delivered",
            LoadStatus.Dispatched => "dispatched",
            _ => @event.StatusToConfirm.ToString().ToLowerInvariant()
        };

        var data = new Dictionary<string, string> { ["loadId"] = @event.LoadId.ToString() };

        // Notify main driver
        if (!string.IsNullOrEmpty(@event.MainDriverDeviceToken))
        {
            await pushNotificationService.SendNotificationAsync(
                "Confirm load status",
                $"You can confirm the {statusText} date of load #{@event.LoadNumber}",
                @event.MainDriverDeviceToken,
                data);
        }

        // Notify secondary driver
        if (!string.IsNullOrEmpty(@event.SecondaryDriverDeviceToken))
        {
            await pushNotificationService.SendNotificationAsync(
                "Confirm load status",
                $"You can confirm the {statusText} date of load #{@event.LoadNumber}",
                @event.SecondaryDriverDeviceToken,
                data);
        }
    }
}
