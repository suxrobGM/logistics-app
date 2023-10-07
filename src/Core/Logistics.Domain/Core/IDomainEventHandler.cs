using MediatR;

namespace Logistics.Domain.Core;

public interface IDomainEventHandler<in T> : INotificationHandler<T> where T : IDomainEvent
{
}