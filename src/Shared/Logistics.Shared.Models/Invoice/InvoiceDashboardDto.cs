namespace Logistics.Shared.Models;

/// <summary>
/// Invoice dashboard statistics.
/// </summary>
public record InvoiceDashboardDto
{
    /// <summary>
    /// Total number of draft invoices.
    /// </summary>
    public int DraftCount { get; set; }

    /// <summary>
    /// Total amount of draft invoices.
    /// </summary>
    public decimal DraftAmount { get; set; }

    /// <summary>
    /// Total number of pending/issued invoices.
    /// </summary>
    public int PendingCount { get; set; }

    /// <summary>
    /// Total amount of pending invoices.
    /// </summary>
    public decimal PendingAmount { get; set; }

    /// <summary>
    /// Total number of overdue invoices.
    /// </summary>
    public int OverdueCount { get; set; }

    /// <summary>
    /// Total amount of overdue invoices.
    /// </summary>
    public decimal OverdueAmount { get; set; }

    /// <summary>
    /// Total number of partially paid invoices.
    /// </summary>
    public int PartiallyPaidCount { get; set; }

    /// <summary>
    /// Total remaining amount on partially paid invoices.
    /// </summary>
    public decimal PartiallyPaidAmount { get; set; }

    /// <summary>
    /// Total number of paid invoices.
    /// </summary>
    public int PaidCount { get; set; }

    /// <summary>
    /// Total amount of paid invoices.
    /// </summary>
    public decimal PaidAmount { get; set; }

    /// <summary>
    /// Total outstanding amount (pending + overdue + partially paid remaining).
    /// </summary>
    public decimal TotalOutstanding { get; set; }

    /// <summary>
    /// Total collected this month.
    /// </summary>
    public decimal CollectedThisMonth { get; set; }

    /// <summary>
    /// Recent invoices for the dashboard.
    /// </summary>
    public List<InvoiceDto> RecentInvoices { get; set; } = [];
}
