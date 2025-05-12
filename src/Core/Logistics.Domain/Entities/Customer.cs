using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

/// <summary>
/// Company's customer (e.g. broker, shipper, etc.).
/// </summary>
public class Customer : Entity, ITenantEntity
{
    public string Name { get; set; } = null!;

    public virtual List<LoadInvoice> Invoices { get; set; } = [];
}
