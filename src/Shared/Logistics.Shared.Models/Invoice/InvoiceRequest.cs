using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

/// <summary>
/// Request body for creating a payment link.
/// </summary>
public record CreatePaymentLinkRequest
{
    /// <summary>
    /// Number of days until the link expires. Defaults to 30 days.
    /// </summary>
    public int? ExpiresInDays { get; set; }
}

/// <summary>
/// Request body for recording a manual payment.
/// </summary>
public record RecordManualPaymentRequest
{
    /// <summary>
    /// The payment amount.
    /// </summary>
    public required decimal Amount { get; set; }

    /// <summary>
    /// The payment type (Cash or Check).
    /// </summary>
    public required PaymentMethodType Type { get; set; }

    /// <summary>
    /// Reference number (e.g., check number).
    /// </summary>
    public string? ReferenceNumber { get; set; }

    /// <summary>
    /// Additional notes about the payment.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Date when the payment was received. Defaults to now if not specified.
    /// </summary>
    public DateTime? ReceivedDate { get; set; }
}

/// <summary>
/// Request body for sending an invoice.
/// </summary>
public record SendInvoiceRequest
{
    /// <summary>
    /// The recipient's email address.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Optional personal message to include in the email.
    /// </summary>
    public string? PersonalMessage { get; set; }
}

/// <summary>
/// Request body for adding a line item to an invoice.
/// </summary>
public record AddLineItemRequest
{
    /// <summary>
    /// Description of the line item.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Type of the line item.
    /// </summary>
    public required InvoiceLineItemType Type { get; set; }

    /// <summary>
    /// Amount per unit.
    /// </summary>
    public required decimal Amount { get; set; }

    /// <summary>
    /// Quantity. Defaults to 1.
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Optional notes.
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Request body for updating a line item on an invoice.
/// </summary>
public record UpdateLineItemRequest
{
    /// <summary>
    /// Description of the line item.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Type of the line item.
    /// </summary>
    public InvoiceLineItemType? Type { get; set; }

    /// <summary>
    /// Amount per unit.
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// Quantity.
    /// </summary>
    public int? Quantity { get; set; }

    /// <summary>
    /// Optional notes.
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Request body for approving a payroll invoice.
/// </summary>
public record ApprovePayrollRequest
{
    /// <summary>
    /// Optional approval notes.
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Request body for rejecting a payroll invoice.
/// </summary>
public record RejectPayrollRequest
{
    /// <summary>
    /// Reason for rejection.
    /// </summary>
    public required string Reason { get; set; }
}

/// <summary>
/// Request body for batch approving payroll invoices.
/// </summary>
public record BatchApprovePayrollRequest
{
    /// <summary>
    /// List of payroll invoice IDs to approve.
    /// </summary>
    public required List<Guid> Ids { get; set; }

    /// <summary>
    /// Optional approval notes.
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Request body for batch creating payroll invoices.
/// </summary>
public record BatchCreatePayrollInvoicesRequest
{
    /// <summary>
    /// List of employee IDs to create payrolls for.
    /// </summary>
    public required List<Guid> EmployeeIds { get; set; }

    /// <summary>
    /// Start date of the payroll period.
    /// </summary>
    public required DateTime PeriodStart { get; set; }

    /// <summary>
    /// End date of the payroll period.
    /// </summary>
    public required DateTime PeriodEnd { get; set; }
}
