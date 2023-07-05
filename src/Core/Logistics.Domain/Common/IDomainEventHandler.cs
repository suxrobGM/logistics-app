using MediatR;

namespace Logistics.Domain.Common;

public interface IDomainEventHandler<in T> : INotificationHandler<T> where T : IDomainEvent
{
}