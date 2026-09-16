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
        var behaviours = serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>().ToArray();

        var chain = (RequestHandlerDelegate<TResponse>)Handler;

        // Folded back to front, so the first-registered behaviour ends up outermost.
        for (var i = behaviours.Length - 1; i >= 0; i--)
        {
            var behaviour = behaviours[i];
            var next = chain;
            chain = ct => behaviour.Handle(typedRequest, next, ct);
        }

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
