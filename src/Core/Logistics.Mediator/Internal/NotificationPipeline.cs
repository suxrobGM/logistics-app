using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Mediator.Internal;

internal sealed class NotificationPipeline<TNotification> : NotificationPipelineBase
    where TNotification : INotification
{
    public override Task Invoke(
        INotification notification,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var handlers = serviceProvider.GetServices<INotificationHandler<TNotification>>().ToArray();

        // Most domain events have no handler, so skip the state machine entirely for them.
        return handlers.Length == 0
            ? Task.CompletedTask
            : InvokeCore(handlers, (TNotification)notification, cancellationToken);
    }

    private static async Task InvokeCore(
        INotificationHandler<TNotification>[] handlers,
        TNotification notification,
        CancellationToken cancellationToken)
    {
        // Sequential: handlers share the DbContext that is mid-SaveChanges.
        foreach (var handler in handlers)
        {
            await handler.Handle(notification, cancellationToken).ConfigureAwait(false);
        }
    }
}
