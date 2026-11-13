using Logistics.Application.Modules.Operations.Notifications.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Events;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;

namespace Logistics.Application.Modules.Operations.Trips.Events;

/// <summary>
/// Tells the new driver about the trip, the old driver when it moved trucks, and the portal and driver
/// chats either way.
/// </summary>
internal sealed class TripAssignedToTruckNotificationHandler(IEventNotifier notifier, ITenantUnitOfWork tenantUow)
    : INotificationHandler<TripAssignedToTruckEvent>
{
    public async Task Handle(TripAssignedToTruckEvent @event, CancellationToken cancellationToken)
    {
        var isReassignment = @event.OldTruckId.HasValue;

        // Read the number off the tracked trip: the event is raised before the insert assigns it.
        var trip = await tenantUow.Repository<Trip>().GetByIdAsync(@event.TripId, cancellationToken);
        var tripNumber = trip?.Number;

        await notifier.PushAsync("New trip assigned",
            $"Trip #{tripNumber} '{@event.TripName}' has been assigned to you", null, @event.NewDriverDeviceToken);

        if (isReassignment)
        {
            await notifier.PushAsync("Trip reassigned",
                $"Trip #{tripNumber} '{@event.TripName}' has been reassigned to another truck", null,
                @event.OldDriverDeviceToken);
        }

        var title = isReassignment ? "Trip reassigned" : "Trip assigned";
        await notifier.ShowInAppAsync(title, $"Trip #{tripNumber} has been assigned to {@event.NewDriverDisplayName}");
        await notifier.PostToTelegramAsync(title,
            $"Trip #{tripNumber} '{@event.TripName}' has been assigned to {@event.NewDriverDisplayName}",
            TelegramChatRole.Driver, cancellationToken);
    }
}
