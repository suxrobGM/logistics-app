using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Events;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Events;

/// <summary>
/// Handles notifications when a load is removed from a truck.
/// Sends push notification to all drivers on the truck.
/// </summary>
internal sealed class LoadRemovedFromTruckNotificationHandler(
    IPushNotificationService pushNotificationService,
    ITelegramNotificationService telegramNotificationService,
    ITenantUnitOfWork tenantUow,
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

        // Send Telegram notification to drivers
        try
        {
            await telegramNotificationService.SendNotificationAsync(
                tenantUow.GetCurrentTenant().Id,
                "Load Removed",
                $"Load #{@event.LoadNumber} has been removed from truck",
                TelegramChatRole.Driver,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send Telegram notification for load removal");
        }
    }
}
