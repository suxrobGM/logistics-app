using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetTripTimelineHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetTripTimelineQuery, Result<TripTimelineDto>>
{
    public async Task<Result<TripTimelineDto>> Handle(
        GetTripTimelineQuery req, CancellationToken ct)
    {
        var trip = await tenantUow.Repository<Trip>().GetByIdAsync(req.TripId, ct);

        if (trip is null)
        {
            return Result<TripTimelineDto>.Fail($"Could not find a trip with ID '{req.TripId}'");
        }

        var events = BuildTimelineEvents(trip);
        var timeline = new TripTimelineDto
        {
            Events = events.OrderBy(e => e.Timestamp)
        };

        return Result<TripTimelineDto>.Ok(timeline);
    }

    private static List<TripTimelineEventDto> BuildTimelineEvents(Trip trip)
    {
        var events = new List<TripTimelineEventDto>
        {
            new()
            {
                Timestamp = trip.CreatedAt,
                EventType = "created",
                Description = $"Trip '{trip.Name}' was created"
            }
        };

        if (trip.DispatchedAt.HasValue)
        {
            events.Add(new TripTimelineEventDto
            {
                Timestamp = trip.DispatchedAt.Value,
                EventType = "dispatched",
                Description = trip.Truck is not null
                    ? $"Trip dispatched to truck {trip.Truck.Number}"
                    : "Trip dispatched"
            });
        }

        // Add stop arrival events
        foreach (var stop in trip.Stops.Where(s => s.ArrivedAt.HasValue).OrderBy(s => s.ArrivedAt))
        {
            var stopType = stop.Type == TripStopType.PickUp ? "picked up" : "delivered";
            events.Add(new TripTimelineEventDto
            {
                Timestamp = stop.ArrivedAt!.Value,
                EventType = stop.Type == TripStopType.PickUp ? "pickup" : "delivery",
                Description = $"Load '{stop.Load.Name}' {stopType}",
                StopId = stop.Id,
                LoadId = stop.LoadId,
                LoadName = stop.Load.Name
            });
        }

        if (trip.CompletedAt.HasValue)
        {
            events.Add(new TripTimelineEventDto
            {
                Timestamp = trip.CompletedAt.Value,
                EventType = "completed",
                Description = "Trip completed successfully"
            });
        }

        if (trip.CancelledAt.HasValue)
        {
            events.Add(new TripTimelineEventDto
            {
                Timestamp = trip.CancelledAt.Value,
                EventType = "cancelled",
                Description = "Trip was cancelled"
            });
        }

        return events;
    }
}
