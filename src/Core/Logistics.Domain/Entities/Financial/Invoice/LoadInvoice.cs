using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public partial class LoadInvoice : Invoice
{
    /// <summary>A draft with the load's charge lines; it is issued when the load is dispatched.</summary>
    public static LoadInvoice Create(Load load)
    {
        var invoice = new LoadInvoice
        {
            Subtotal = load.DeliveryCost,
            TaxTotal = 0m,
            Total = load.DeliveryCost,
            Status = InvoiceStatus.Draft,
            CustomerId = load.CustomerId,
            Customer = load.Customer,
            LoadId = load.Id,
            Load = load
        };

        invoice.ReplaceLoadCharges(load);
        invoice.RecalculateTotals();
        return invoice;
    }

    public override InvoiceType Type { get; set; } = InvoiceType.Load;
    public required Guid LoadId { get; set; }
    public virtual Load Load { get; set; } = null!;

    public required Guid CustomerId { get; set; }
    public virtual Customer Customer { get; set; } = null!;
}
