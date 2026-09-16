namespace Logistics.Mediator;

/// <summary>
/// Dispatches requests to their single handler and notifications to every registered handler.
/// </summary>
public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes to every handler registered for the notification's runtime type, awaiting each in
    /// turn. Publishing a notification with no handlers does nothing.
    /// </summary>
    Task Publish(INotification notification, CancellationToken cancellationToken = default);
}
