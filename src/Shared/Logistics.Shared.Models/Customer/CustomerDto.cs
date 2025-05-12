namespace Logistics.Shared.Models;

public class CustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    // public IEnumerable<InvoiceDto> Invoices { get; set; } = new List<InvoiceDto>();
}
