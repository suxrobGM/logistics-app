namespace Logistics.Shared.Models;

public class InvoiceDto
{
    public string Id { get; set; } = default!;
    public ulong LoadRef { get; set; }
    public string LoadId { get; set; } = default!;
    public DateTime CreatedDate { get; set; }
    public CustomerDto Customer { get; set; } = default!;
    public PaymentDto Payment { get; set; } = default!;
}
