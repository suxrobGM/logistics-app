using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Mediator.Internal;

internal sealed class NotificationPipeline<TNotification> : NotificationPipeline
    where TNotification : INotification
{
    public override async Task Invoke(
        INotification notification,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var typed = (TNotification)notification;

        foreach (var handler in serviceProvider.GetServices<INotificationHandler<TNotification>>())
        {
            await handler.Handle(typed, cancellationToken).ConfigureAwait(false);
        }
    }
}
