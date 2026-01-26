using Logistics.Domain.Core;

namespace Logistics.Domain.Events;

/// <summary>
/// Raised when a payroll invoice is submitted for approval.
/// Contains all data needed for notifications to avoid additional queries.
/// </summary>
public record PayrollSubmittedEvent(
    Guid PayrollId,
    long PayrollNumber,
    Guid EmployeeId,
    string EmployeeName,
    string EmployeeEmail,
    decimal TotalAmount,
    string Currency,
    DateTime PeriodStart,
    DateTime PeriodEnd) : IDomainEvent;
