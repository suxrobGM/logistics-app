using Logistics.Application.Abstractions;
using Logistics.Domain.Events;

using Microsoft.Extensions.Logging;

namespace Logistics.Application.Events;

internal sealed class TripDispatchedHandler : IDomainEventHandler<TripDispatchedEvent>
{
    private readonly ILogger<TripDispatchedHandler> _logger;

    public TripDispatchedHandler(ILogger<TripDispatchedHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(TripDispatchedEvent @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Trip dispatched: {TripId}", @event.TripId);
        return Task.CompletedTask;
    }
}
