using Logistics.Domain.Core;

namespace Logistics.Domain.Events;

public record EmployeeDocumentUploadedEvent(Guid DocId, Guid EmployeeId) : IDomainEvent;
