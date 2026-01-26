namespace Logistics.Application.Services.EmailSender;

/// <summary>
/// Email model for payroll approved notification.
/// </summary>
public record PayrollApprovedEmailModel
{
    public required string EmployeeName { get; init; }
    public required long PayrollNumber { get; init; }
    public required string TotalAmount { get; init; }
    public required string Currency { get; init; }
    public required string PeriodStart { get; init; }
    public required string PeriodEnd { get; init; }
}
