using Logistics.Domain.Abstractions;

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
    
    /// <summary>
    /// Percentage of income that drivers can receive from total gross income. Value must be in range [0, 1]
    /// </summary>
    public float DriverIncomePercentage { get; set; }
    
    public virtual List<Employee> Drivers { get; set; } = new();
    public virtual List<Load> Loads { get; } = new();

    public static Truck Create(string truckNumber, float driverIncomePercentage, Employee driver)
    {
        return Create(truckNumber, driverIncomePercentage, new []{driver});
    }
    
    public static Truck Create(string truckNumber, float driverIncomePercentage, IEnumerable<Employee> drivers)
    {
        var newTruck = new Truck
        {
            TruckNumber = truckNumber,
            DriverIncomePercentage = driverIncomePercentage,
        };

        foreach (var driver in drivers)
        {
            newTruck.Drivers.Add(driver);
        }
        
        return newTruck;
    }
}
