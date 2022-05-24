namespace Logistics.Domain.Entities;

public class Truck : Entity, ITenantEntity
{
    public int? TruckNumber { get; set; }
    public string? DriverId { get; set; }

    public virtual User? Driver { get; set; }
    public virtual IList<Cargo> Cargoes { get; set; } = new List<Cargo>();
}