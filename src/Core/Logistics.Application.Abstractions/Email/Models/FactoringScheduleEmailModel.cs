namespace Logistics.Application.Abstractions.Email.Models;

/// <summary>
/// Model for the schedule-of-accounts email sent to a factoring company.
/// </summary>
public record FactoringScheduleEmailModel
{
    public required string CompanyName { get; init; }
    public required string FactorName { get; init; }
    public required long ScheduleNumber { get; init; }
    public required string Total { get; init; }
    public string? Notes { get; init; }
    public string? ReplyEmail { get; init; }
    public List<FactoringScheduleInvoiceEmailModel> Invoices { get; init; } = [];
}

public record FactoringScheduleInvoiceEmailModel
{
    public required long InvoiceNumber { get; init; }
    public required long LoadNumber { get; init; }
    public required string CustomerName { get; init; }
    public required string Amount { get; init; }
}
