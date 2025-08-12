using Logistics.Domain.Core;

using MediatR;

using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Logistics.Infrastructure.Interceptors;

public class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IMediator _mediator;

    public DispatchDomainEventsInterceptor(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var context = eventData.Context;
        if (context is not null)
        {
            DispatchDomainEvents(context.ChangeTracker).GetAwaiter().GetResult();
        }

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
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
        var domainEventEntities = changeTracker.Entries<Entity>()
            .Select(i => i.Entity)
            .Where(i => i.DomainEvents.Any())
            .ToArray();

        foreach (var entity in domainEventEntities)
        {
            foreach (var entityDomainEvent in entity.DomainEvents)
            {
                await _mediator.Publish(entityDomainEvent, cancellationToken);
            }

            entity.DomainEvents.Clear();
        }
    }
}
