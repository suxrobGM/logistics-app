using Logistics.Domain.Core;

namespace Logistics.Domain.Events;

public record LoadDocumentDeletedEvent(Guid DocumentId, Guid LoadId) : IDomainEvent;