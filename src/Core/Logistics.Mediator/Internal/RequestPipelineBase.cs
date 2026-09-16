namespace Logistics.Mediator.Internal;

/// <summary>
/// Type-erased entry point to a closed <see cref="RequestPipeline{TRequest,TResponse}" />, so the
/// mediator can cache one per request type without naming the response type.
/// </summary>
internal abstract class RequestPipelineBase
{
    public abstract Task<object?> Invoke(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}
