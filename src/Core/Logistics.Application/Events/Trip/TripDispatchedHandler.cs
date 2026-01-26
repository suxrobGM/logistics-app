using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Events;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Events;

internal sealed class TripDispatchedHandler(
    ILogger<TripDispatchedHandler> logger,
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUserService,
    ITripTrackingService tripTrackingService)
    : IDomainEventHandler<TripDispatchedEvent>
{
    public async Task Handle(TripDispatchedEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Trip dispatched: {TripId}", @event.TripId);

        var trip = await tenantUow.Repository<Trip>().GetByIdAsync(@event.TripId, cancellationToken);
        if (trip is null)
        {
            logger.LogWarning("Trip not found for dispatched event: {TripId}", @event.TripId);
            return;
        }

        var tenantId = tenantUow.GetCurrentTenant().Id;

        // Broadcast trip status update
        await tripTrackingService.BroadcastTripStatusUpdateAsync(
            tenantId,
            new TripStatusUpdateDto
            {
                TripId = @event.TripId,
                TripName = trip.Name,
                Status = TripStatus.Dispatched,
                PreviousStatus = TripStatus.Draft,
                UpdatedAt = DateTime.UtcNow
            });

        // Broadcast dispatch board update
        await tripTrackingService.BroadcastDispatchBoardUpdateAsync(
            tenantId,
            new DispatchBoardUpdateDto
            {
                UpdateType = DispatchBoardUpdateType.TripDispatched,
                EntityId = @event.TripId,
                UpdatedAt = DateTime.UtcNow
            });
    }
}
