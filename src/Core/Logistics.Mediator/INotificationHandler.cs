namespace Logistics.Mediator;

/// <summary>
/// Handles a published <typeparamref name="TNotification" />. Any number of handlers may be
/// registered for one notification; they run in registration order, one at a time.
/// </summary>
public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
    Task Handle(TNotification notification, CancellationToken cancellationToken);
}
