namespace Logistics.Shared.Models;

public class InvoiceDto
{
    public string? CompanyName { get; set; }
    public string? CompanyAddress { get; set; }
    public string LoadId { get; set; } = default!;
    public CustomerDto Customer { get; set; } = default!;
    public PaymentDto Payment { get; set; } = default!;
}
