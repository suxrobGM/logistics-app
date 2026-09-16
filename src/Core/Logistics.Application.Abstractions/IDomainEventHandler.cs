using Logistics.Domain.Core;
using Logistics.Mediator;

namespace Logistics.Application.Abstractions;

public interface IDomainEventHandler<in T> : INotificationHandler<T> where T : IDomainEvent;
