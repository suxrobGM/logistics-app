using Logistics.Domain.Core;

namespace Logistics.Domain.Events;

public record TruckDocumentUploadedEvent(Guid DocId, Guid TruckId) : IDomainEvent;
