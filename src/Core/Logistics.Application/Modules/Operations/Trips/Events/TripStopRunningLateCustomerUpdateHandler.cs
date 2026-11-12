using Logistics.Application.Modules.Operations.Loads.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Events;
using Logistics.Domain.Persistence;
using Logistics.Mediator;

namespace Logistics.Application.Modules.Operations.Trips.Events;

internal sealed class TripStopRunningLateCustomerUpdateHandler(
    ITenantUnitOfWork tenantUow,
    ICustomerUpdateService customerUpdates)
    : INotificationHandler<TripStopRunningLateEvent>
{
    public async Task Handle(TripStopRunningLateEvent @event, CancellationToken cancellationToken)
    {
        var stop = await tenantUow.Repository<TripStop>().GetByIdAsync(@event.StopId, cancellationToken);
        if (stop is null)
        {
            return;
        }

        await customerUpdates.SendLateNoticeAsync(stop, @event.EstimatedArrivalAt, cancellationToken);
    }
}
