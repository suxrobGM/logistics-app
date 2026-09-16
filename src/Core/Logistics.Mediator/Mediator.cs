using System.Collections.Concurrent;
using Logistics.Mediator.Internal;

namespace Logistics.Mediator;

/// <inheritdoc />
public sealed class Mediator(IServiceProvider serviceProvider) : IMediator
{
    // Pipelines are stateless and built by reflection, so caching them statically means a
    // transient Mediator never re-pays MakeGenericType.
    private static readonly ConcurrentDictionary<Type, RequestPipeline> RequestPipelines = new();
    private static readonly ConcurrentDictionary<Type, NotificationPipeline> NotificationPipelines = new();

    /// <inheritdoc />
    public async Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await GetRequestPipeline(request.GetType())
            .Invoke(request, serviceProvider, cancellationToken)
            .ConfigureAwait(false);

        return (TResponse)result!;
    }

    /// <inheritdoc />
    public Task<object?> Send(IBaseRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return GetRequestPipeline(request.GetType()).Invoke(request, serviceProvider, cancellationToken);
    }

    /// <inheritdoc />
    public Task Publish(INotification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        // Keyed on the runtime type, never on a static one: callers routinely hold the
        // notification as a marker interface, and keying on that would dispatch to nothing.
        var pipeline = NotificationPipelines.GetOrAdd(notification.GetType(), static notificationType =>
        {
            var closed = typeof(NotificationPipeline<>).MakeGenericType(notificationType);
            return (NotificationPipeline)Activator.CreateInstance(closed)!;
        });

        return pipeline.Invoke(notification, serviceProvider, cancellationToken);
    }

    private static RequestPipeline GetRequestPipeline(Type requestType) =>
        RequestPipelines.GetOrAdd(requestType, static type =>
        {
            // Taking the response type from the request rather than from the caller's type
            // argument is what makes the single-Type cache key safe under IRequest covariance.
            var responseType = Array.Find(
                    type.GetInterfaces(),
                    i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))
                ?.GetGenericArguments()[0]
                ?? throw new ArgumentException($"'{type}' does not implement IRequest<TResponse>.", nameof(requestType));

            var closed = typeof(RequestPipeline<,>).MakeGenericType(type, responseType);
            return (RequestPipeline)Activator.CreateInstance(closed)!;
        });
}
