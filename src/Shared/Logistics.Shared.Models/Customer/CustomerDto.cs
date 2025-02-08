namespace Logistics.Shared.Models;

public class CustomerDto
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    // public IEnumerable<InvoiceDto> Invoices { get; set; } = new List<InvoiceDto>();
}
