using MediatR;

namespace Logistics.Domain.Abstractions;

public interface IDomainEventHandler<in T> : INotificationHandler<T> where T : IDomainEvent
{
}