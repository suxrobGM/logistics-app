using Logistics.Mediator.Tests.TestKit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Logistics.Mediator.Tests;

public class MediatorPublishTests
{
    private readonly Trace _trace = new();

    private IMediator Build(Func<Type, bool> typeFilter)
    {
        var services = new ServiceCollection();
        services.AddSingleton(_trace);
        services.AddMediator(typeof(Ping).Assembly, typeFilter);
        return services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    private IMediator BuildWithoutThrowingListener() =>
        Build(type => type != typeof(SecondDuplicateHandler) && type != typeof(ThrowingListener));

    /// <summary>
    ///     The production caller holds an IDomainEvent, never the concrete event, so dispatch must
    ///     key on the runtime type. Keying on the static type reaches no handler and throws nothing.
    ///     The trace also pins that handlers are awaited one at a time.
    /// </summary>
    [Fact]
    public async Task Publish_NotificationTypedAsMarkerInterface_DispatchesToEachHandlerInTurn()
    {
        ITestEvent notification = new ThingHappened();

        await BuildWithoutThrowingListener().Publish(notification, CancellationToken.None);

        Assert.Equal(["first>", "first<", "second>", "second<"], _trace.Entries);
    }

    /// <summary>Most domain events have no handler, and every save publishes them all.</summary>
    [Fact]
    public async Task Publish_NoHandlersRegistered_DoesNotThrow()
    {
        await BuildWithoutThrowingListener().Publish(new NobodyCares(), CancellationToken.None);

        Assert.Empty(_trace.Entries);
    }

    [Fact]
    public async Task Publish_HandlerThrows_ExceptionPropagates()
    {
        var sut = Build(type => type == typeof(ThrowingListener));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.Publish(new ThingHappened(), CancellationToken.None));
    }
}
