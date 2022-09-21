namespace Logistics.Domain.Entities;

public class Truck : Entity, ITenantEntity
{
    public int TruckNumber { get; set; } = 100;
    public string? DriverId { get; set; }

    public virtual Employee? Driver { get; set; }
    public virtual IList<Load> Loads { get; } = new List<Load>();
}