using Logistics.Domain.Core;

namespace Logistics.Domain.Events;

/// <summary>
/// Raised when a payroll invoice is rejected.
/// Contains all data needed for notifications to avoid additional queries.
/// </summary>
public record PayrollRejectedEvent(
    Guid PayrollId,
    long PayrollNumber,
    Guid EmployeeId,
    string EmployeeName,
    string EmployeeEmail,
    string? EmployeeDeviceToken,
    string RejectionReason,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    Guid RejectedById) : IDomainEvent;
