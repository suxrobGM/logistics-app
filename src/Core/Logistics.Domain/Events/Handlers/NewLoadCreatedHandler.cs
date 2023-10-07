using Logistics.Domain.Core;
using Microsoft.Extensions.Logging;

namespace Logistics.Domain.Events.Handlers;

internal sealed class NewLoadCreatedHandler : IDomainEventHandler<NewLoadCreatedEvent>
{
    private readonly ILogger<NewLoadCreatedHandler> _logger;

    public NewLoadCreatedHandler(ILogger<NewLoadCreatedHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(NewLoadCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Created a new load #{@LoadRefId}", notification.LoadRefId);
        return Task.CompletedTask;
    }
}
