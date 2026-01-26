using Logistics.Domain.Core;

namespace Logistics.Domain.Events;

/// <summary>
/// Raised when a payment is recorded against a payroll invoice.
/// Contains all data needed for notifications to avoid additional queries.
/// </summary>
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
    string Currency,
    bool IsFullyPaid,
    DateTime PeriodStart,
    DateTime PeriodEnd) : IDomainEvent;
