namespace Logistics.Shared.Models;

public class InvoiceDto
{
    public string? CompanyName { get; set; }
    public string? CompanyAddress { get; set; }
    public string LoadId { get; set; } = default!;
    public string CustomerId { get; set; } = default!;
    public PaymentDto Payment { get; set; } = default!;
}
