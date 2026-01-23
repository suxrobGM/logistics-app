using Logistics.Application.Abstractions;
using Logistics.Domain.Events;

using Microsoft.Extensions.Logging;

namespace Logistics.Application.Events;

internal sealed class TripCompletedHandler(ILogger<TripCompletedHandler> logger)
    : IDomainEventHandler<TripCompletedEvent>
{
    public Task Handle(TripCompletedEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Trip completed: {TripId}", @event.TripId);
        return Task.CompletedTask;
    }
}
