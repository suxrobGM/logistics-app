using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Events;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Notifications;

namespace Logistics.Application.Modules.Operations.Trips.Events;

/// <summary>
/// Handles notifications when a trip is assigned to a truck.
/// Sends push notification to new driver, notification to old driver (if reassigned),
/// and in-app notification to TMS portal users.
/// </summary>
internal sealed class TripAssignedToTruckNotificationHandler(
    IPushNotificationService pushNotificationService,
    INotificationService notificationService,
    ITelegramNotificationService telegramNotificationService,
    ITenantUnitOfWork tenantUow,
    ILogger<TripAssignedToTruckNotificationHandler> logger)
    : INotificationHandler<TripAssignedToTruckEvent>
{
    public async Task Handle(TripAssignedToTruckEvent @event, CancellationToken cancellationToken)
    {
        var isReassignment = @event.OldTruckId.HasValue;

        logger.LogInformation(
            "Trip {TripId} {Action} truck {TruckId}, sending notifications",
            @event.TripId,
            isReassignment ? "reassigned to" : "assigned to",
            @event.NewTruckId);

        // Read the number off the tracked trip: the event is raised before the insert assigns it.
        var trip = await tenantUow.Repository<Trip>().GetByIdAsync(@event.TripId, cancellationToken);
        var tripNumber = trip?.Number;

        if (!string.IsNullOrEmpty(@event.NewDriverDeviceToken))
        {
            await pushNotificationService.SendNotificationAsync(
                "New trip assigned",
                $"Trip #{tripNumber} '{@event.TripName}' has been assigned to you",
                @event.NewDriverDeviceToken);
        }

        if (isReassignment && !string.IsNullOrEmpty(@event.OldDriverDeviceToken))
        {
            await pushNotificationService.SendNotificationAsync(
                "Trip reassigned",
                $"Trip #{tripNumber} '{@event.TripName}' has been reassigned to another truck",
                @event.OldDriverDeviceToken);
        }

        var title = isReassignment ? "Trip reassigned" : "Trip assigned";
        await notificationService.SendNotificationAsync(
            title,
            $"Trip #{tripNumber} has been assigned to {@event.NewDriverDisplayName}");

        try
        {
            await telegramNotificationService.SendNotificationAsync(
                tenantUow.GetCurrentTenant().Id,
                title,
                $"Trip #{tripNumber} '{@event.TripName}' has been assigned to {@event.NewDriverDisplayName}",
                TelegramChatRole.Driver,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send Telegram notification for trip assignment");
        }
    }
}
