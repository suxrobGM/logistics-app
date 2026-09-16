using System.Collections.Concurrent;
using Logistics.Mediator.Internal;

namespace Logistics.Mediator;

/// <inheritdoc />
public sealed class Mediator(IServiceProvider serviceProvider) : IMediator
{
    // Pipelines are stateless and built by reflection, so caching them statically means a
    // transient Mediator never re-pays MakeGenericType.
    private static readonly ConcurrentDictionary<Type, RequestPipelineBase> RequestPipelines = new();
    private static readonly ConcurrentDictionary<Type, NotificationPipelineBase> NotificationPipelines = new();

    /// <inheritdoc />
    public async Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Keyed on the runtime type, never TResponse: the caller routinely holds the request as a
        // marker interface, and the handler is registered against the concrete type.
        var result = await RequestPipelines
            .GetOrAdd(request.GetType(), CreateRequestPipeline)
            .Invoke(request, serviceProvider, cancellationToken)
            .ConfigureAwait(false);

        return (TResponse)result!;
    }

    /// <inheritdoc />
    public Task Publish(INotification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var pipeline = NotificationPipelines.GetOrAdd(notification.GetType(), static notificationType =>
        {
            var closed = typeof(NotificationPipeline<>).MakeGenericType(notificationType);
            return (NotificationPipelineBase)Activator.CreateInstance(closed)!;
        });

        return pipeline.Invoke(notification, serviceProvider, cancellationToken);
    }

    private static RequestPipelineBase CreateRequestPipeline(Type requestType)
    {
        var responseType = Array.Find(
                requestType.GetInterfaces(),
                i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))
            ?.GetGenericArguments()[0]
            ?? throw new ArgumentException($"'{requestType}' does not implement IRequest<TResponse>.", nameof(requestType));

        var closed = typeof(RequestPipeline<,>).MakeGenericType(requestType, responseType);
        return (RequestPipelineBase)Activator.CreateInstance(closed)!;
    }
}
