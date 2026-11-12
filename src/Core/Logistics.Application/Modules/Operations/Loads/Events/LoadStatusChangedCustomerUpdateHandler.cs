using Logistics.Application.Modules.Operations.Loads.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Events;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;

namespace Logistics.Application.Modules.Operations.Loads.Events;

/// <summary>
/// Emails the customer when their load is dispatched, picked up or delivered.
/// </summary>
internal sealed class LoadStatusChangedCustomerUpdateHandler(
    ITenantUnitOfWork tenantUow,
    ICustomerUpdateService customerUpdates)
    : INotificationHandler<LoadStatusChangedEvent>
{
    public async Task Handle(LoadStatusChangedEvent @event, CancellationToken cancellationToken)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(@event.LoadId, cancellationToken);
        if (load is null)
        {
            return;
        }

        // Arriving straight at a drop-off picks up and delivers in one save; only the delivery is news.
        var supersededByDelivery = @event.NewStatus == LoadStatus.PickedUp && load.Status == LoadStatus.Delivered;
        if (supersededByDelivery)
        {
            return;
        }

        await customerUpdates.SendStatusUpdateAsync(load, @event.NewStatus, cancellationToken);
    }
}
