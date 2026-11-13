using Logistics.Domain.Core;

namespace Logistics.Domain.Events;

/// <summary>Raised when a payroll invoice is approved. Carries the fields notifications need, so handlers make no extra queries.</summary>
public record PayrollApprovedEvent(
    Guid PayrollId,
    long PayrollNumber,
    Guid EmployeeId,
    string EmployeeName,
    string EmployeeEmail,
    string? EmployeeDeviceToken,
    decimal TotalAmount,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    Guid ApprovedById) : IDomainEvent;
