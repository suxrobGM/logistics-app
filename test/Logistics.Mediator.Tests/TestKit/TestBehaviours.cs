namespace Logistics.Mediator.Tests.TestKit;

public sealed class OuterBehaviour<TRequest, TResponse>(Trace trace) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        trace.Add("A>");
        var response = await next(cancellationToken);
        trace.Add("A<");
        return response;
    }
}

public sealed class InnerBehaviour<TRequest, TResponse>(Trace trace) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        trace.Add("B>");
        var response = await next(cancellationToken);
        trace.Add("B<");
        return response;
    }
}

public sealed class ShortCircuitBehaviour<TRequest>(Trace trace) : IPipelineBehavior<TRequest, string>
    where TRequest : notnull
{
    public Task<string> Handle(
        TRequest request,
        RequestHandlerDelegate<string> next,
        CancellationToken cancellationToken)
    {
        trace.Add("denied");
        return Task.FromResult("short-circuited");
    }
}
