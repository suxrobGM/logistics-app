using Logistics.Mediator.Tests.TestKit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Logistics.Mediator.Tests;

public class MediatorPublishTests
{
    private readonly Trace _trace = new();

    private IMediator Build(Func<Type, bool>? typeFilter = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton(_trace);
        services.AddMediator(
            typeof(Ping).Assembly,
            typeFilter ?? (type => type != typeof(SecondDuplicateHandler) && type != typeof(ThrowingListener)));
        return services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    /// <summary>
    ///     The production caller holds an IDomainEvent, never the concrete event. Keying dispatch on
    ///     the static type instead of the runtime type would silently reach no handler at all.
    /// </summary>
    [Fact]
    public async Task Publish_NotificationTypedAsMarkerInterface_DispatchesByRuntimeType()
    {
        var sut = Build();
        ITestEvent notification = new ThingHappened();

        await sut.Publish(notification, CancellationToken.None);

        Assert.Contains("first>", _trace.Entries);
        Assert.Contains("second>", _trace.Entries);
    }

    [Fact]
    public async Task Publish_MultipleHandlers_AwaitsEachBeforeStartingTheNext()
    {
        var sut = Build();

        await sut.Publish(new ThingHappened(), CancellationToken.None);

        Assert.Equal(["first>", "first<", "second>", "second<"], _trace.Entries);
    }

    [Fact]
    public async Task Publish_NoHandlersRegistered_DoesNotThrow()
    {
        var sut = Build();

        await sut.Publish(new NobodyCares(), CancellationToken.None);

        Assert.Empty(_trace.Entries);
    }

    [Fact]
    public async Task Publish_HandlerThrows_ExceptionPropagates()
    {
        var sut = Build(type => type == typeof(ThrowingListener));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.Publish(new ThingHappened(), CancellationToken.None));
    }

    [Fact]
    public async Task Publish_NullNotification_Throws()
    {
        var sut = Build();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.Publish(null!, CancellationToken.None));
    }
}
