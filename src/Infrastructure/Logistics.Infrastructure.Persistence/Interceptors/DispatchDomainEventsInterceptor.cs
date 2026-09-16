using Logistics.Domain.Core;
using Logistics.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Logistics.Infrastructure.Persistence.Interceptors;

public class DispatchDomainEventsInterceptor(IMediator mediator) : SaveChangesInterceptor
{
    /// <summary>
    ///     Contexts currently dispatching, so a handler that saves again does not recurse.
    ///     Tracked per context rather than per flow: one scoped instance serves both the master and
    ///     the tenant context, and a flow-wide flag would swallow the second context's events.
    /// </summary>
    private readonly HashSet<DbContext> _dispatching = [];

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var response = await base.SavingChangesAsync(eventData, result, cancellationToken);

        // Dispatch after the save so handlers querying these entities find them in the database.
        if (eventData.Context is not null)
        {
            await DispatchDomainEvents(eventData.Context, cancellationToken);
        }

        return response;
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
                    await mediator.Publish(domainEvent, cancellationToken);
                }
            }
        }
        finally
        {
            _dispatching.Remove(context);
        }
    }
}
