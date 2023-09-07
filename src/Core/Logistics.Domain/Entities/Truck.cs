namespace Logistics.Domain.Entities;

public class Truck : AuditableEntity, ITenantEntity
{
    public string? TruckNumber { get; set; }
    public virtual List<Employee> Drivers { get; set; } = new();
    public virtual List<Load> Loads { get; } = new();
}
