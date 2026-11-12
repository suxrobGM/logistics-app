using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Modules.Operations.Loads.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Events;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;

namespace Logistics.Application.Modules.Operations.Loads.Events;

/// <summary>Queued, because the send is SMTP round trips and would hold up the request.</summary>
internal sealed class LoadStatusChangedCustomerUpdateHandler(
    ITenantUnitOfWork tenantUow,
    ICommandEnqueuer commandEnqueuer)
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

        commandEnqueuer.Enqueue(new SendCustomerStatusUpdateCommand
        {
            TenantId = tenantUow.GetCurrentTenant().Id,
            LoadId = load.Id,
            NewStatus = @event.NewStatus
        });
    }
}
