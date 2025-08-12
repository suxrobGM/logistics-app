using Logistics.Application.Abstractions;
using Logistics.Domain.Events;

using Microsoft.Extensions.Logging;

namespace Logistics.Application.Events;

internal sealed class NewTripCreatedHandler : IDomainEventHandler<NewTripCreatedEvent>
{
    private readonly ILogger<NewTripCreatedHandler> _logger;

    public NewTripCreatedHandler(ILogger<NewTripCreatedHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(NewTripCreatedEvent @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Created a new trip {Id}", @event.TripId);
        return Task.CompletedTask;
    }
}
