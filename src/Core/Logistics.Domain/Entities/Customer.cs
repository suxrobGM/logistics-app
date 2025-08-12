using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

/// <summary>
/// Company's customer (e.g. broker, shipper, etc.).
/// </summary>
public class Customer : Entity, ITenantEntity
{
    public required string Name { get; set; }

    public virtual List<LoadInvoice> Invoices { get; set; } = [];
}
