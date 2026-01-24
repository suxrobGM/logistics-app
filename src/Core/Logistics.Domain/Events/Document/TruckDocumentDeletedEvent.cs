using Logistics.Domain.Core;

namespace Logistics.Domain.Events;

public record TruckDocumentDeletedEvent(Guid Id, Guid TruckId) : IDomainEvent;
