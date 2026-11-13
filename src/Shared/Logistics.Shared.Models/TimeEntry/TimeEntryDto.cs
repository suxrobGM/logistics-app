namespace Logistics.Shared.Models;

public class TimeEntryDto
{
    public string Id { get; set; } = default!;
    public string EmployeeId { get; set; } = default!;
    public string? EmployeeName { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime EndedAt { get; set; }
    public decimal TotalHours { get; set; }
    public string Type { get; set; } = default!;
    public string? PayrollInvoiceId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
