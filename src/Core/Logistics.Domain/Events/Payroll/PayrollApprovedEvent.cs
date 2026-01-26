using Logistics.Domain.Core;

namespace Logistics.Domain.Events;

/// <summary>
/// Raised when a payroll invoice is approved.
/// Contains all data needed for notifications to avoid additional queries.
/// </summary>
public record PayrollApprovedEvent(
    Guid PayrollId,
    long PayrollNumber,
    Guid EmployeeId,
    string EmployeeName,
    string EmployeeEmail,
    string? EmployeeDeviceToken,
    decimal TotalAmount,
    string Currency,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    Guid ApprovedById) : IDomainEvent;
