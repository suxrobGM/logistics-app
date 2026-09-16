namespace Logistics.Mediator.Internal;

/// <summary>
/// Type-erased entry point to a closed <see cref="NotificationPipeline{TNotification}" />, so the
/// mediator can cache one per notification runtime type.
/// </summary>
internal abstract class NotificationPipelineBase
{
    public abstract Task Invoke(
        INotification notification,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}
