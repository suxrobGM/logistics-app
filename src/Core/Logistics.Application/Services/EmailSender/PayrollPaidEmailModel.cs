namespace Logistics.Application.Services.EmailSender;

/// <summary>
/// Email model for payroll payment notification.
/// </summary>
public record PayrollPaidEmailModel
{
    public required string EmployeeName { get; init; }
    public required long PayrollNumber { get; init; }
    public required string PaymentAmount { get; init; }
    public required string TotalAmount { get; init; }
    public required string OutstandingAmount { get; init; }
    public required string Currency { get; init; }
    public required bool IsFullyPaid { get; init; }
    public required string PeriodStart { get; init; }
    public required string PeriodEnd { get; init; }
}
