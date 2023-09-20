namespace Logistics.Domain.Entities;

public class Truck : AuditableEntity, ITenantEntity
{
    public string? TruckNumber { get; set; }
    
    /// <summary>
    /// Truck last known location address
    /// </summary>
    public string? LastKnownLocation { get; set; }
    
    /// <summary>
    /// Truck last known location latitude
    /// </summary>
    public double? LastKnownLocationLat { get; set; }
    
    /// <summary>
    /// Truck last known location longitude
    /// </summary>
    public double? LastKnownLocationLong { get; set; }
    
    public virtual List<Employee> Drivers { get; set; } = new();
    public virtual List<Load> Loads { get; } = new();
}
