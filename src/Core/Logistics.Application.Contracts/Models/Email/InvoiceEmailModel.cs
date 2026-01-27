namespace Logistics.Application.Contracts.Models.Email;

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
    public required string PaymentUrl { get; init; }
    public required string ExpiresAt { get; init; }
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
