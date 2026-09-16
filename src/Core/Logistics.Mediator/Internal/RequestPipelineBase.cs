namespace Logistics.Mediator.Internal;

/// <summary>
/// Lets the mediator cache one pipeline per request type in a single dictionary. The request type
/// alone does not name the response type, so the cache cannot be typed any tighter than this.
/// </summary>
internal abstract class RequestPipelineBase;

/// <summary>
/// The response-typed entry point a <c>Send&lt;TResponse&gt;</c> call casts back down to, so
/// dispatch returns the handler's own Task rather than an erased one.
/// </summary>
internal abstract class RequestPipelineBase<TResponse> : RequestPipelineBase
{
    public abstract Task<TResponse> Invoke(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}
