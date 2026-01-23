using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Events;

/// <summary>
/// Handles notifications when a trip is assigned to a truck.
/// Sends push notification to new driver, notification to old driver (if reassigned),
/// and in-app notification to TMS portal users.
/// </summary>
internal sealed class TripAssignedToTruckNotificationHandler(
    IPushNotificationService pushNotificationService,
    INotificationService notificationService,
    ILogger<TripAssignedToTruckNotificationHandler> logger)
    : IDomainEventHandler<TripAssignedToTruckEvent>
{
    public async Task Handle(TripAssignedToTruckEvent @event, CancellationToken cancellationToken)
    {
        var isReassignment = @event.OldTruckId.HasValue;

        logger.LogInformation(
            "Trip {TripId} {Action} truck {TruckId}, sending notifications",
            @event.TripId,
            isReassignment ? "reassigned to" : "assigned to",
            @event.NewTruckId);

        // Send push notification to new driver
        if (!string.IsNullOrEmpty(@event.NewDriverDeviceToken))
        {
            await pushNotificationService.SendNotificationAsync(
                "New trip assigned",
                $"Trip #{@event.TripNumber} '{@event.TripName}' has been assigned to you",
                @event.NewDriverDeviceToken);
        }

        // If reassigned, notify the old driver
        if (isReassignment && !string.IsNullOrEmpty(@event.OldDriverDeviceToken))
        {
            await pushNotificationService.SendNotificationAsync(
                "Trip reassigned",
                $"Trip #{@event.TripNumber} '{@event.TripName}' has been reassigned to another truck",
                @event.OldDriverDeviceToken);
        }

        // Send in-app notification for TMS portal users
        var title = isReassignment ? "Trip reassigned" : "Trip assigned";
        await notificationService.SendNotificationAsync(
            title,
            $"Trip #{@event.TripNumber} has been assigned to {@event.NewDriverDisplayName}");
    }
}
