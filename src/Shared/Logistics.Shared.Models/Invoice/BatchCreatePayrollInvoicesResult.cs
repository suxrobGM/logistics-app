namespace Logistics.Shared.Models;

/// <summary>
/// Result of batch creating payroll invoices.
/// </summary>
public record BatchCreatePayrollInvoicesResult
{
    /// <summary>
    /// List of successfully created invoice IDs.
    /// </summary>
    public List<Guid> CreatedInvoiceIds { get; set; } = [];

    /// <summary>
    /// List of errors for failed invoice creations.
    /// </summary>
    public List<BatchCreatePayrollError> Errors { get; set; } = [];
}

/// <summary>
/// Error details for a failed payroll invoice creation.
/// </summary>
public record BatchCreatePayrollError(Guid EmployeeId, string Message);
