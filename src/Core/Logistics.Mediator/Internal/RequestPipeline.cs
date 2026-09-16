using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Mediator.Internal;

internal sealed class RequestPipeline<TRequest, TResponse> : RequestPipelineBase
    where TRequest : IRequest<TResponse>
{
    public override async Task<object?> Invoke(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var typedRequest = (TRequest)request;

        // GetServices yields registration order, so reversing before the fold leaves the
        // first-registered behaviour outermost. Without the Reverse the pipeline runs inside out.
        var chain = serviceProvider
            .GetServices<IPipelineBehavior<TRequest, TResponse>>()
            .Reverse()
            .Aggregate(
                (RequestHandlerDelegate<TResponse>)Handler,
                (next, behaviour) => ct => behaviour.Handle(typedRequest, next, ct));

        return await chain(cancellationToken).ConfigureAwait(false);

        Task<TResponse> Handler(CancellationToken ct)
        {
            var handler = serviceProvider.GetService<IRequestHandler<TRequest, TResponse>>()
                ?? throw new InvalidOperationException(
                    $"No handler is registered for request '{typeof(TRequest)}'. Expected a registration for " +
                    $"'{typeof(IRequestHandler<TRequest, TResponse>)}'. Check that the handler's assembly is passed " +
                    "to AddMediator and that the type filter does not exclude it.");

            return handler.Handle(typedRequest, ct);
        }
    }
}
