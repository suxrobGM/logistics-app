using Logistics.Domain.Core;
using Logistics.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Persistence.Interceptors;

/// <summary>
///     Publishes domain events once the save has committed, so handlers see database-generated
///     values (load and trip numbers) and never act on a save that then fails. Inside an explicit
///     transaction "committed" means "written, not yet committed".
/// </summary>
public class DispatchDomainEventsInterceptor(
    IMediator mediator,
    ILogger<DispatchDomainEventsInterceptor> logger) : SaveChangesInterceptor
{
    /// <summary>
    ///     Contexts currently dispatching, so a handler that saves again does not recurse.
    ///     Tracked per context rather than per flow: one scoped instance serves both the master and
    ///     the tenant context, and a flow-wide flag would swallow the second context's events.
    /// </summary>
    private readonly HashSet<DbContext> _dispatching = [];

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await DispatchDomainEvents(eventData.Context, cancellationToken);
        }

        return result;
    }

    private async Task DispatchDomainEvents(DbContext context, CancellationToken cancellationToken)
    {
        if (!_dispatching.Add(context))
        {
            return;
        }

        try
        {
            // Loop until quiet: a handler may raise further events on this context.
            while (true)
            {
                var entities = context.ChangeTracker.Entries<Entity>()
                    .Select(i => i.Entity)
                    .Where(i => i.DomainEvents.Count > 0)
                    .ToArray();

                if (entities.Length == 0)
                {
                    break;
                }

                // Clear before dispatching, or the next pass re-publishes the same events.
                var events = new List<IDomainEvent>();
                foreach (var entity in entities)
                {
                    events.AddRange(entity.DomainEvents);
                    entity.DomainEvents.Clear();
                }

                foreach (var domainEvent in events)
                {
                    await PublishAsync(domainEvent, cancellationToken);
                }

                // Handlers used to run inside the save, so anything they tracked was written with it.
                if (context.ChangeTracker.HasChanges())
                {
                    await context.SaveChangesAsync(cancellationToken);
                }
            }
        }
        finally
        {
            _dispatching.Remove(context);
        }
    }

    /// <summary>
    ///     The data is already committed, so a failing handler must not fail the request - the
    ///     client would retry and write the change twice.
    /// </summary>
    private async Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            await mediator.Publish(domainEvent, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Domain event handler failed for {EventType}", domainEvent.GetType().Name);
        }
    }
}
