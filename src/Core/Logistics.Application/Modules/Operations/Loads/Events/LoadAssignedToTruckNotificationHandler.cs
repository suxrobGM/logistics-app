using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Events;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Notifications;

namespace Logistics.Application.Modules.Operations.Loads.Events;

/// <summary>
/// Handles notifications when a load is assigned to a truck.
/// Sends push notification to driver and in-app notification to TMS portal users.
/// </summary>
internal sealed class LoadAssignedToTruckNotificationHandler(
    IPushNotificationService pushNotificationService,
    INotificationService notificationService,
    ITelegramNotificationService telegramNotificationService,
    ITenantUnitOfWork tenantUow,
    ILogger<LoadAssignedToTruckNotificationHandler> logger)
    : INotificationHandler<LoadAssignedToTruckEvent>
{
    public async Task Handle(LoadAssignedToTruckEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Load {LoadId} assigned to truck {TruckId}, sending notifications",
            @event.LoadId, @event.TruckId);

        // Read the number off the tracked load: the event is raised before the insert assigns it.
        var load = await tenantUow.Repository<Load>().GetByIdAsync(@event.LoadId, cancellationToken);
        var loadNumber = load?.Number;
        var data = new Dictionary<string, string> { ["loadId"] = @event.LoadId.ToString() };

        if (!string.IsNullOrEmpty(@event.MainDriverDeviceToken))
        {
            await pushNotificationService.SendNotificationAsync(
                "Received a load",
                $"A new load #{loadNumber} has been assigned to you",
                @event.MainDriverDeviceToken,
                data);
        }

        if (!string.IsNullOrEmpty(@event.SecondaryDriverDeviceToken))
        {
            await pushNotificationService.SendNotificationAsync(
                "Received a load",
                $"A new load #{loadNumber} has been assigned to you",
                @event.SecondaryDriverDeviceToken,
                data);
        }

        await notificationService.SendNotificationAsync(
            "Load assigned",
            $"Load #{loadNumber} has been assigned to {@event.DriverDisplayName}");

        try
        {
            await telegramNotificationService.SendNotificationAsync(
                tenantUow.GetCurrentTenant().Id,
                "Load Assigned",
                $"Load #{loadNumber} has been assigned to {@event.DriverDisplayName}",
                TelegramChatRole.Driver,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send Telegram notification for load assignment");
        }
    }
}
