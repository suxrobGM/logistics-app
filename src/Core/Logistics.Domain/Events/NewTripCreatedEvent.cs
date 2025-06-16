using Logistics.Domain.Core;

namespace Logistics.Domain.Events;

public record NewTripCreatedEvent(Guid TripId) : IDomainEvent;