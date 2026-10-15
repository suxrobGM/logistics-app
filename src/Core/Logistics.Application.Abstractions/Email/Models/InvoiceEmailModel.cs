namespace Logistics.Application.Abstractions.Email.Models;

/// <summary>
/// Model for invoice email templates.
/// </summary>
public record InvoiceEmailModel
{
    public required string CompanyName { get; init; }
    public required long InvoiceNumber { get; init; }
    public required string Total { get; init; }
    public required string AmountDue { get; init; }
    public required string Currency { get; init; }
    public string? DueDate { get; init; }
    public string? CustomerName { get; init; }
    public long? LoadNumber { get; init; }
    public string? PersonalMessage { get; init; }

    /// <summary>
    /// Null for a factored invoice, which is paid to the factor instead (see <see cref="RemitToName"/>).
    /// </summary>
    public string? PaymentUrl { get; init; }
    public string? ExpiresAt { get; init; }

    /// <summary>
    /// Factoring company the invoice was assigned to; the email then carries the notice of assignment.
    /// </summary>
    public string? RemitToName { get; init; }

    public string? RemitToAddress { get; init; }

    public List<InvoiceLineItemEmailModel> LineItems { get; init; } = [];
}

/// <summary>
/// Line item model for invoice emails.
/// </summary>
public record InvoiceLineItemEmailModel
{
    public required string Description { get; init; }
    public required string Amount { get; init; }
    public int Quantity { get; init; } = 1;
    public required string Total { get; init; }
}
