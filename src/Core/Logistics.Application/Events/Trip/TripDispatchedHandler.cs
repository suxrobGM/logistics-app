using Logistics.Application.Abstractions;
using Logistics.Domain.Events;

using Microsoft.Extensions.Logging;

namespace Logistics.Application.Events;

internal sealed class TripDispatchedHandler(ILogger<TripDispatchedHandler> logger)
    : IDomainEventHandler<TripDispatchedEvent>
{
    public Task Handle(TripDispatchedEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Trip dispatched: {TripId}", @event.TripId);
        return Task.CompletedTask;
    }
}
