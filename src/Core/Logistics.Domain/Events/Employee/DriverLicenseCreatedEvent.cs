using Logistics.Domain.Core;

namespace Logistics.Domain.Events;

public record DriverLicenseCreatedEvent(Guid LicenseId, Guid EmployeeId) : IDomainEvent;
