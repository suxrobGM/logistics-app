namespace Logistics.Mediator.Tests.TestKit;

/// <summary>Stands in for IDomainEvent: the type callers hold rather than the concrete event.</summary>
public interface ITestEvent : INotification;

public sealed record ThingHappened : ITestEvent;

public sealed record NobodyCares : ITestEvent;

public sealed class FirstListener(Trace trace) : INotificationHandler<ThingHappened>
{
    public Task Handle(ThingHappened notification, CancellationToken cancellationToken)
    {
        trace.Add("first>");
        return Task.Run(() => trace.Add("first<"), cancellationToken);
    }
}

public sealed class SecondListener(Trace trace) : INotificationHandler<ThingHappened>
{
    public Task Handle(ThingHappened notification, CancellationToken cancellationToken)
    {
        trace.Add("second>");
        return Task.Run(() => trace.Add("second<"), cancellationToken);
    }
}

public sealed class ThrowingListener : INotificationHandler<ThingHappened>
{
    public Task Handle(ThingHappened notification, CancellationToken cancellationToken) =>
        throw new InvalidOperationException("listener failed");
}
