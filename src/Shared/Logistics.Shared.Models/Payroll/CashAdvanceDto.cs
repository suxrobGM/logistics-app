using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

public record CashAdvanceDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public required Money Amount { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>The settlement it was deducted from; null while still owed.</summary>
    public Guid? PayrollInvoiceId { get; set; }
}
