using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Events;

/// <summary>
/// Handles notifications when load details are updated.
/// Sends push notification to all drivers on the truck.
/// </summary>
internal sealed class LoadUpdatedNotificationHandler(
    IPushNotificationService pushNotificationService,
    ILogger<LoadUpdatedNotificationHandler> logger)
    : IDomainEventHandler<LoadUpdatedEvent>
{
    public async Task Handle(LoadUpdatedEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Load {LoadId} updated, sending notifications to truck {TruckId}",
            @event.LoadId, @event.TruckId);

        var data = new Dictionary<string, string> { ["loadId"] = @event.LoadId.ToString() };

        // Notify main driver
        if (!string.IsNullOrEmpty(@event.MainDriverDeviceToken))
        {
            await pushNotificationService.SendNotificationAsync(
                "Load update",
                $"Load #{@event.LoadNumber} details have been updated. Check details.",
                @event.MainDriverDeviceToken,
                data);
        }

        // Notify secondary driver
        if (!string.IsNullOrEmpty(@event.SecondaryDriverDeviceToken))
        {
            await pushNotificationService.SendNotificationAsync(
                "Load update",
                $"Load #{@event.LoadNumber} details have been updated. Check details.",
                @event.SecondaryDriverDeviceToken,
                data);
        }
    }
}
