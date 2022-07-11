namespace Logistics.Domain.Entities;

public class Truck : Entity, ITenantEntity
{
    public int? TruckNumber { get; set; }
    public string? DriverId { get; set; }

    public virtual Employee? Driver { get; set; }
    public virtual IList<Load> Cargoes { get; set; } = new List<Load>();
}