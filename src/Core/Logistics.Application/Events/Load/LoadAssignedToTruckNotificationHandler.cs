using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Events;

/// <summary>
/// Handles notifications when a load is assigned to a truck.
/// Sends push notification to driver and in-app notification to TMS portal users.
/// </summary>
internal sealed class LoadAssignedToTruckNotificationHandler(
    IPushNotificationService pushNotificationService,
    INotificationService notificationService,
    ILogger<LoadAssignedToTruckNotificationHandler> logger)
    : IDomainEventHandler<LoadAssignedToTruckEvent>
{
    public async Task Handle(LoadAssignedToTruckEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Load {LoadId} assigned to truck {TruckId}, sending notifications",
            @event.LoadId, @event.TruckId);

        var data = new Dictionary<string, string> { ["loadId"] = @event.LoadId.ToString() };

        // Send push notification to main driver
        if (!string.IsNullOrEmpty(@event.MainDriverDeviceToken))
        {
            await pushNotificationService.SendNotificationAsync(
                "Received a load",
                $"A new load #{@event.LoadNumber} has been assigned to you",
                @event.MainDriverDeviceToken,
                data);
        }

        // Send push notification to secondary driver
        if (!string.IsNullOrEmpty(@event.SecondaryDriverDeviceToken))
        {
            await pushNotificationService.SendNotificationAsync(
                "Received a load",
                $"A new load #{@event.LoadNumber} has been assigned to you",
                @event.SecondaryDriverDeviceToken,
                data);
        }

        // Send in-app notification for TMS portal users
        await notificationService.SendNotificationAsync(
            "Load assigned",
            $"Load #{@event.LoadNumber} has been assigned to {@event.DriverDisplayName}");
    }
}
