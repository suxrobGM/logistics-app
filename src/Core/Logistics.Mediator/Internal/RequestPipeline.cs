namespace Logistics.Mediator.Internal;

/// <summary>
/// Non-generic entry point to a closed <see cref="RequestPipeline{TRequest,TResponse}" />, so the
/// mediator can cache one instance per request type without knowing the response type.
/// </summary>
internal abstract class RequestPipeline
{
    public abstract Task<object?> Invoke(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}
