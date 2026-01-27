namespace Logistics.Application.Contracts.Models.Email;

/// <summary>
/// Email model for payroll rejected notification.
/// </summary>
public record PayrollRejectedEmailModel
{
    public required string EmployeeName { get; init; }
    public required long PayrollNumber { get; init; }
    public required string RejectionReason { get; init; }
    public required string PeriodStart { get; init; }
    public required string PeriodEnd { get; init; }
}
