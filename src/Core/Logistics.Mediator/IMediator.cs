namespace Logistics.Mediator;

/// <summary>
/// Dispatches requests to their single handler and notifications to every registered handler.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Sends <paramref name="request" /> through its pipeline and returns the handler's response.
    /// </summary>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a request whose response type is not known statically. Use this only where the
    /// request arrives as a deserialised payload; prefer the generic overload everywhere else.
    /// </summary>
    Task<object?> Send(IBaseRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes <paramref name="notification" /> to every handler registered for its runtime
    /// type, awaiting each in turn. Publishing a notification with no handlers does nothing.
    /// </summary>
    Task Publish(INotification notification, CancellationToken cancellationToken = default);
}
