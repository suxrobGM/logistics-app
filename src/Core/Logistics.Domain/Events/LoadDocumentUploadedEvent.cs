using Logistics.Domain.Core;

namespace Logistics.Domain.Events;

public record LoadDocumentUploadedEvent(Guid DocumentId, Guid LoadId) : IDomainEvent;
