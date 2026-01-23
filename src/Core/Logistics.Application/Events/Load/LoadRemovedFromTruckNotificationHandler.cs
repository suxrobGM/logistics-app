using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Events;

/// <summary>
/// Handles notifications when a load is removed from a truck.
/// Sends push notification to all drivers on the truck.
/// </summary>
internal sealed class LoadRemovedFromTruckNotificationHandler(
    IPushNotificationService pushNotificationService,
    ILogger<LoadRemovedFromTruckNotificationHandler> logger)
    : IDomainEventHandler<LoadRemovedFromTruckEvent>
{
    public async Task Handle(LoadRemovedFromTruckEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Load {LoadId} removed from truck {TruckId}, sending notifications",
            @event.LoadId, @event.TruckId);

        var data = new Dictionary<string, string> { ["loadId"] = @event.LoadId.ToString() };

        // Notify main driver
        if (!string.IsNullOrEmpty(@event.MainDriverDeviceToken))
        {
            await pushNotificationService.SendNotificationAsync(
                "Load update",
                $"Load #{@event.LoadNumber} has been removed from you",
                @event.MainDriverDeviceToken,
                data);
        }

        // Notify secondary driver
        if (!string.IsNullOrEmpty(@event.SecondaryDriverDeviceToken))
        {
            await pushNotificationService.SendNotificationAsync(
                "Load update",
                $"Load #{@event.LoadNumber} has been removed from you",
                @event.SecondaryDriverDeviceToken,
                data);
        }
    }
}
