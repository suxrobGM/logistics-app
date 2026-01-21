using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Events;

public record DeliveryDocumentCreatedEvent(Guid DocumentId, Guid LoadId, DocumentType DocumentType) : IDomainEvent;
