using Logistics.Domain.Core;

using MediatR;

namespace Logistics.Application.Abstractions;

public interface IDomainEventHandler<in T> : INotificationHandler<T> where T : IDomainEvent;
