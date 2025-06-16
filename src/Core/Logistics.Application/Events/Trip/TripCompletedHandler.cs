using Logistics.Application.Abstractions;
using Logistics.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Events;

internal sealed class TripCompletedHandler : IDomainEventHandler<TripCompletedEvent>
{
    private readonly ILogger<TripCompletedHandler> _logger;

    public TripCompletedHandler(ILogger<TripCompletedHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(TripCompletedEvent @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Trip completed: {TripId}", @event.TripId);
        return Task.CompletedTask;
    }
}
