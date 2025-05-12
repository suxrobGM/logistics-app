using Logistics.Shared.Consts;

namespace Logistics.Domain.Entities;

public class LoadInvoice : Invoice
{
    public override InvoiceType Type { get; set; } = InvoiceType.Load;
    public required Guid LoadId { get; set; }
    public virtual Load Load { get; set; } = null!;

    public required Guid CustomerId { get; set; }
    public virtual Customer Customer { get; set; } = null!;
}
