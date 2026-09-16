namespace Logistics.Mediator;

/// <summary>
/// Wraps request handling. Behaviours run in registration order, the first registered being
/// outermost. Returning without awaiting <paramref name="next" /> short-circuits the pipeline and
/// the handler never runs.
/// </summary>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : notnull
{
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken);
}
