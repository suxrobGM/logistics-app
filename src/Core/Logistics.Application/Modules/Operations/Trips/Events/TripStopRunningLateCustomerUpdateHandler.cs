using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Modules.Operations.Loads.Commands;
using Logistics.Domain.Events;
using Logistics.Domain.Persistence;
using Logistics.Mediator;

namespace Logistics.Application.Modules.Operations.Trips.Events;

/// <summary>Queued, because domain events run inside SaveChangesAsync and the send is SMTP round trips.</summary>
internal sealed class TripStopRunningLateCustomerUpdateHandler(
    ITenantUnitOfWork tenantUow,
    ICommandEnqueuer commandEnqueuer)
    : INotificationHandler<TripStopRunningLateEvent>
{
    public Task Handle(TripStopRunningLateEvent @event, CancellationToken cancellationToken)
    {
        commandEnqueuer.Enqueue(new SendCustomerLateNoticeCommand
        {
            TenantId = tenantUow.GetCurrentTenant().Id,
            StopId = @event.StopId,
            EstimatedArrivalAt = @event.EstimatedArrivalAt
        });
        return Task.CompletedTask;
    }
}
