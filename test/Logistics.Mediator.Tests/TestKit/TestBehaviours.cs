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

/// <summary>Returns without awaiting next, the way FeatureCheckBehaviour denies a request.</summary>
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

/// <summary>
///     Counts instantiations per closed generic, mirroring FeatureCheckBehaviour's static attribute
///     lookup. Each (TRequest, TResponse) pair must get its own copy of the static field.
/// </summary>
public sealed class CountingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public static int Count;

    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Count++;
        return next(cancellationToken);
    }
}

/// <summary>Constrained so the container skips it for any response that is not a string.</summary>
public sealed class StringOnlyBehaviour<TRequest, TResponse>(Trace trace) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : class, IComparable<string>
{
    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        trace.Add("string-only");
        return next(cancellationToken);
    }
}
