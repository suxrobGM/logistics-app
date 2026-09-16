namespace Logistics.Mediator.Tests.TestKit;

public sealed record Ping(string Value = "ping") : IRequest<string>;

public sealed record Unhandled : IRequest<string>;

public sealed record Duplicated : IRequest<string>;

public sealed record OtherPing : IRequest<string>;

public sealed class PingHandler(Trace trace) : IRequestHandler<Ping, string>
{
    public Task<string> Handle(Ping request, CancellationToken cancellationToken)
    {
        trace.Add("handler");
        return Task.FromResult(request.Value);
    }
}

// Internal and sealed, matching how the application declares its handlers: the scanner must find
// these through GetTypes rather than GetExportedTypes.
internal sealed class OtherPingHandler : IRequestHandler<OtherPing, string>
{
    public Task<string> Handle(OtherPing request, CancellationToken cancellationToken) =>
        Task.FromResult("other");
}

public sealed class FirstDuplicateHandler : IRequestHandler<Duplicated, string>
{
    public Task<string> Handle(Duplicated request, CancellationToken cancellationToken) =>
        Task.FromResult("first");
}

public sealed class SecondDuplicateHandler : IRequestHandler<Duplicated, string>
{
    public Task<string> Handle(Duplicated request, CancellationToken cancellationToken) =>
        Task.FromResult("second");
}
