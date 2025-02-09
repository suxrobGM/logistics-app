using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

public class Customer : Entity, ITenantEntity
{
    public string Name { get; set; } = null!;

    public virtual List<Invoice> Invoices { get; set; } = new();
}
