using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Events;
using Logistics.Domain.Persistence;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Events;

/// <summary>
/// Handles notifications when load details are updated.
/// Sends push notification to all drivers on the truck.
/// </summary>
internal sealed class LoadUpdatedNotificationHandler(
    IPushNotificationService pushNotificationService,
    ITelegramNotificationService telegramNotificationService,
    ITenantUnitOfWork tenantUow,
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

        // Send Telegram notification to all users (both dispatchers and drivers)
        try
        {
            await telegramNotificationService.SendNotificationAsync(
                tenantUow.GetCurrentTenant().Id,
                "Load Updated",
                $"Load #{@event.LoadNumber} details have been updated",
                targetRole: null,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send Telegram notification for load update");
        }
    }
}
