using Logistics.Domain.Core;

namespace Logistics.Domain.Events;

/// <summary>Raised when a payroll invoice is submitted. Carries the fields notifications need, so handlers make no extra queries.</summary>
public record PayrollSubmittedEvent(
    Guid PayrollId,
    long PayrollNumber,
    Guid EmployeeId,
    string EmployeeName,
    string EmployeeEmail,
    decimal TotalAmount,
    DateOnly PeriodStart,
    DateOnly PeriodEnd) : IDomainEvent;
