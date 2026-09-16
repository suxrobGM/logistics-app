using Logistics.Domain.Core;
using Logistics.Infrastructure.Persistence.Interceptors;
using Logistics.Mediator;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.Persistence.Tests.Interceptors;

public class DispatchDomainEventsInterceptorTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly DispatchDomainEventsInterceptor _sut;

    public DispatchDomainEventsInterceptorTests()
    {
        _sut = new DispatchDomainEventsInterceptor(_mediator);
    }

    private ProbeDbContext NewContext(string name) =>
        new(new DbContextOptionsBuilder<ProbeDbContext>()
            .UseInMemoryDatabase(name)
            .AddInterceptors(_sut)
            .Options);

    [Fact]
    public async Task SavingChangesAsync_EntityWithEvent_PublishesAndClearsIt()
    {
        await using var context = NewContext(Guid.NewGuid().ToString());
        var thing = new Thing();
        thing.DomainEvents.Add(new ThingSaved());
        context.Add(thing);

        await context.SaveChangesAsync(CancellationToken.None);

        await _mediator.Received(1).Publish(Arg.Any<ThingSaved>(), Arg.Any<CancellationToken>());
        Assert.Empty(thing.DomainEvents);
    }

    /// <summary>
    ///     One scoped interceptor serves the master and the tenant context. A flow-wide re-entry
    ///     guard makes the nested save return early and silently drop the second context's events.
    /// </summary>
    [Fact]
    public async Task SavingChangesAsync_HandlerSavesAnotherContext_StillPublishesThatContextsEvents()
    {
        await using var outer = NewContext(Guid.NewGuid().ToString());
        await using var inner = NewContext(Guid.NewGuid().ToString());

        var innerThing = new Thing();
        innerThing.DomainEvents.Add(new ThingSaved());
        inner.Add(innerThing);

        // Stand in for a handler that writes through the other DbContext while the first save is
        // still dispatching.
        _mediator.Publish(Arg.Any<OuterSaved>(), Arg.Any<CancellationToken>())
            .Returns(_ => inner.SaveChangesAsync(CancellationToken.None));

        var outerThing = new Thing();
        outerThing.DomainEvents.Add(new OuterSaved());
        outer.Add(outerThing);

        await outer.SaveChangesAsync(CancellationToken.None);

        await _mediator.Received(1).Publish(Arg.Any<ThingSaved>(), Arg.Any<CancellationToken>());
        Assert.Empty(innerThing.DomainEvents);
    }

    [Fact]
    public async Task SavingChangesAsync_HandlerSavesTheSameContext_DoesNotRecurse()
    {
        await using var context = NewContext(Guid.NewGuid().ToString());

        _mediator.Publish(Arg.Any<ThingSaved>(), Arg.Any<CancellationToken>())
            .Returns(_ => context.SaveChangesAsync(CancellationToken.None));

        var thing = new Thing();
        thing.DomainEvents.Add(new ThingSaved());
        context.Add(thing);

        await context.SaveChangesAsync(CancellationToken.None);

        await _mediator.Received(1).Publish(Arg.Any<ThingSaved>(), Arg.Any<CancellationToken>());
    }

    private sealed record ThingSaved : IDomainEvent;

    private sealed record OuterSaved : IDomainEvent;

    private sealed class Thing : Entity;

    private sealed class ProbeDbContext(DbContextOptions<ProbeDbContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.Entity<Thing>();
    }
}
