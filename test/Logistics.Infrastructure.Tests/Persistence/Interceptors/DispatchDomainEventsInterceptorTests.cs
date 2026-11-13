using Logistics.Domain.Core;
using Logistics.Infrastructure.Persistence.Interceptors;
using Logistics.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Logistics.Infrastructure.Tests.Persistence.Interceptors;

public class DispatchDomainEventsInterceptorTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly DispatchDomainEventsInterceptor _sut;

    public DispatchDomainEventsInterceptorTests()
    {
        _sut = new DispatchDomainEventsInterceptor(_mediator, NullLogger<DispatchDomainEventsInterceptor>.Instance);
    }

    private ProbeDbContext NewContext(string name, bool withInterceptor = true)
    {
        var options = new DbContextOptionsBuilder<ProbeDbContext>().UseInMemoryDatabase(name);
        if (withInterceptor)
        {
            options.AddInterceptors(_sut);
        }

        return new ProbeDbContext(options.Options);
    }

    [Fact]
    public async Task SavedChangesAsync_EntityWithEvent_PublishesAndClearsIt()
    {
        await using var context = NewContext(Guid.NewGuid().ToString());
        var thing = AddThing(context, new ThingSaved());

        await context.SaveChangesAsync(CancellationToken.None);

        await _mediator.Received(1).Publish(Arg.Any<ThingSaved>(), Arg.Any<CancellationToken>());
        Assert.Empty(thing.DomainEvents);
    }

    /// <summary>Handlers read database-generated values (load and trip numbers), so the row must exist first.</summary>
    [Fact]
    public async Task SavedChangesAsync_PublishesAfterRowIsPersisted()
    {
        var database = Guid.NewGuid().ToString();
        await using var context = NewContext(database);
        var thing = AddThing(context, new ThingSaved());

        var persistedWhenPublished = false;
        _mediator.Publish(Arg.Any<ThingSaved>(), Arg.Any<CancellationToken>())
            .Returns(async _ =>
            {
                await using var probe = NewContext(database, withInterceptor: false);
                persistedWhenPublished = await probe.Things.AnyAsync(t => t.Id == thing.Id);
            });

        await context.SaveChangesAsync(CancellationToken.None);

        Assert.True(persistedWhenPublished);
    }

    [Fact]
    public async Task SavedChangesAsync_HandlerAddsEntityWithoutSaving_PersistsIt()
    {
        var database = Guid.NewGuid().ToString();
        await using var context = NewContext(database);
        var added = new Thing();
        _mediator.Publish(Arg.Any<ThingSaved>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                context.Add(added);
                return Task.CompletedTask;
            });

        AddThing(context, new ThingSaved());

        await context.SaveChangesAsync(CancellationToken.None);

        await using var probe = NewContext(database, withInterceptor: false);
        Assert.True(await probe.Things.AnyAsync(t => t.Id == added.Id));
    }

    [Fact]
    public async Task SavedChangesAsync_HandlerAddsEntityWithEvent_PublishesThatEventToo()
    {
        await using var context = NewContext(Guid.NewGuid().ToString());
        _mediator.Publish(Arg.Any<ThingSaved>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                AddThing(context, new OuterSaved());
                return Task.CompletedTask;
            });

        AddThing(context, new ThingSaved());

        await context.SaveChangesAsync(CancellationToken.None);

        await _mediator.Received(1).Publish(Arg.Any<OuterSaved>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SavedChangesAsync_SaveFails_DoesNotPublish()
    {
        var database = Guid.NewGuid().ToString();
        var id = Guid.NewGuid();
        await using (var seed = NewContext(database, withInterceptor: false))
        {
            seed.Add(new Thing { Id = id });
            await seed.SaveChangesAsync(CancellationToken.None);
        }

        await using var context = NewContext(database);
        var duplicate = new Thing { Id = id };
        duplicate.DomainEvents.Add(new ThingSaved());
        context.Add(duplicate);

        await Assert.ThrowsAnyAsync<Exception>(() => context.SaveChangesAsync(CancellationToken.None));

        await _mediator.DidNotReceive().Publish(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
        Assert.Single(duplicate.DomainEvents);
    }

    /// <summary>The data is committed by then; failing the request would invite a retry that writes it twice.</summary>
    [Fact]
    public async Task SavedChangesAsync_HandlerThrows_SaveStillReturnsAndNextEventPublished()
    {
        await using var context = NewContext(Guid.NewGuid().ToString());
        _mediator.Publish(Arg.Any<ThingSaved>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("push failed"));

        AddThing(context, new ThingSaved(), new OuterSaved());

        var written = await context.SaveChangesAsync(CancellationToken.None);

        Assert.Equal(1, written);
        await _mediator.Received(1).Publish(Arg.Any<OuterSaved>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     One scoped interceptor serves the master and the tenant context. A flow-wide re-entry
    ///     guard makes the nested save return early and silently drop the second context's events.
    /// </summary>
    [Fact]
    public async Task SavedChangesAsync_HandlerSavesAnotherContext_StillPublishesThatContextsEvents()
    {
        await using var outer = NewContext(Guid.NewGuid().ToString());
        await using var inner = NewContext(Guid.NewGuid().ToString());

        var innerThing = AddThing(inner, new ThingSaved());

        // Stand in for a handler that writes through the other DbContext while the first save is
        // still dispatching.
        _mediator.Publish(Arg.Any<OuterSaved>(), Arg.Any<CancellationToken>())
            .Returns(_ => inner.SaveChangesAsync(CancellationToken.None));

        AddThing(outer, new OuterSaved());

        await outer.SaveChangesAsync(CancellationToken.None);

        await _mediator.Received(1).Publish(Arg.Any<ThingSaved>(), Arg.Any<CancellationToken>());
        Assert.Empty(innerThing.DomainEvents);
    }

    [Fact]
    public async Task SavedChangesAsync_HandlerSavesTheSameContext_DoesNotRecurse()
    {
        await using var context = NewContext(Guid.NewGuid().ToString());

        _mediator.Publish(Arg.Any<ThingSaved>(), Arg.Any<CancellationToken>())
            .Returns(_ => context.SaveChangesAsync(CancellationToken.None));

        AddThing(context, new ThingSaved());

        await context.SaveChangesAsync(CancellationToken.None);

        await _mediator.Received(1).Publish(Arg.Any<ThingSaved>(), Arg.Any<CancellationToken>());
    }

    private static Thing AddThing(DbContext context, params IDomainEvent[] events)
    {
        var thing = new Thing();
        thing.DomainEvents.AddRange(events);
        context.Add(thing);
        return thing;
    }

    private sealed record ThingSaved : IDomainEvent;

    private sealed record OuterSaved : IDomainEvent;

    private sealed class Thing : Entity;

    private sealed class ProbeDbContext(DbContextOptions<ProbeDbContext> options) : DbContext(options)
    {
        public DbSet<Thing> Things => Set<Thing>();

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.Entity<Thing>();
    }
}
