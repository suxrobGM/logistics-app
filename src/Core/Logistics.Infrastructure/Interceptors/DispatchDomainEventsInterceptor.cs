using Logistics.Domain.Core;

using MediatR;

using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Logistics.Infrastructure.Interceptors;

public class DispatchDomainEventsInterceptor(IMediator mediator) : SaveChangesInterceptor
{
    /// <summary>
    /// Re-entry guard to prevent infinite loops when handlers call SaveChangesAsync.
    /// Uses AsyncLocal to be safe in async contexts.
    /// </summary>
    private static readonly AsyncLocal<bool> isDispatching = new();

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var context = eventData.Context;
        if (context is not null)
        {
            DispatchDomainEvents(context.ChangeTracker).GetAwaiter().GetResult();
        }

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        // Save first, then dispatch events
        // This ensures entities exist in DB when handlers query them
        var response = await base.SavingChangesAsync(eventData, result, cancellationToken);

        var context = eventData.Context;
        if (context is not null)
        {
            await DispatchDomainEvents(context.ChangeTracker, cancellationToken);
        }

        return response;
    }

    private async Task DispatchDomainEvents(ChangeTracker changeTracker, CancellationToken cancellationToken = default)
    {
        // Re-entry guard: if we're already dispatching, skip to prevent infinite loops
        if (isDispatching.Value)
        {
            return;
        }

        try
        {
            isDispatching.Value = true;

            // Loop until no more events are raised (handlers may raise new events)
            while (true)
            {
                var domainEventEntities = changeTracker.Entries<Entity>()
                    .Select(i => i.Entity)
                    .Where(i => i.DomainEvents.Any())
                    .ToArray();

                if (domainEventEntities.Length == 0)
                {
                    break;
                }

                // Collect all events and clear them BEFORE dispatching to prevent re-dispatch
                var allEvents = new List<IDomainEvent>();
                foreach (var entity in domainEventEntities)
                {
                    allEvents.AddRange(entity.DomainEvents);
                    entity.DomainEvents.Clear();
                }

                // Now dispatch all collected events
                foreach (var domainEvent in allEvents)
                {
                    await mediator.Publish(domainEvent, cancellationToken);
                }
            }
        }
        finally
        {
            isDispatching.Value = false;
        }
    }
}
