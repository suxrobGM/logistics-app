using Logistics.Domain.Core;

namespace Logistics.Domain.Events;

public record EmployeeDocumentDeletedEvent(Guid Id, Guid EmployeeId) : IDomainEvent;
