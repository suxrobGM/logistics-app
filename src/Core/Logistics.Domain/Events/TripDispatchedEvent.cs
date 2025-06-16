using Logistics.Domain.Core;

namespace Logistics.Domain.Events;

public record TripDispatchedEvent(Guid TripId) : IDomainEvent;
