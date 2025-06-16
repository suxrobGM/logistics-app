using Logistics.Domain.Core;

namespace Logistics.Domain.Events;

public record TripCompletedEvent(Guid TripId) : IDomainEvent;