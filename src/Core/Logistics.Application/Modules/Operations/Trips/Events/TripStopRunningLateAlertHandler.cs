using Logistics.Application.Modules.Operations.Trips.Services;
using Logistics.Domain.Events;
using Logistics.Mediator;

namespace Logistics.Application.Modules.Operations.Trips.Events;

/// <summary>
/// Hands a late stop to the dispatch agent as a trip alert, beside the customer email.
/// </summary>
internal sealed class TripStopRunningLateAlertHandler(ITripAlertReporter alertReporter)
    : INotificationHandler<TripStopRunningLateEvent>
{
    public Task Handle(TripStopRunningLateEvent @event, CancellationToken cancellationToken) =>
        alertReporter.ReportLateStopAsync(@event, cancellationToken);
}
