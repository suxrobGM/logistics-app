using Logistics.Domain.Core;

namespace Logistics.Domain.Events;

/// <summary>Raised when a payroll invoice is paid. Carries the fields notifications need, so handlers make no extra queries.</summary>
public record PayrollPaidEvent(
    Guid PayrollId,
    long PayrollNumber,
    Guid EmployeeId,
    string EmployeeName,
    string EmployeeEmail,
    string? EmployeeDeviceToken,
    decimal PaymentAmount,
    decimal TotalAmount,
    decimal OutstandingAmount,
    bool IsFullyPaid,
    DateOnly PeriodStart,
    DateOnly PeriodEnd) : IDomainEvent;
