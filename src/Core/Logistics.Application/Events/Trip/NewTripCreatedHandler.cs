using Logistics.Application.Abstractions;
using Logistics.Domain.Events;

using Microsoft.Extensions.Logging;

namespace Logistics.Application.Events;

internal sealed class NewTripCreatedHandler(ILogger<NewTripCreatedHandler> logger)
    : IDomainEventHandler<NewTripCreatedEvent>
{
    public Task Handle(NewTripCreatedEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Created a new trip {Id}", @event.TripId);
        return Task.CompletedTask;
    }
}
