namespace Logistics.Mediator.Internal;

/// <summary>
/// Non-generic entry point to a closed <see cref="NotificationPipeline{TNotification}" />, so the
/// mediator can cache one instance per notification runtime type.
/// </summary>
internal abstract class NotificationPipeline
{
    public abstract Task Invoke(
        INotification notification,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}
