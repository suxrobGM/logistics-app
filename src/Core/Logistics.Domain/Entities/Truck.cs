using Logistics.Domain.Core;
using Logistics.Domain.ValueObjects;
using Logistics.Shared.Enums;

namespace Logistics.Domain.Entities;

public class Truck : AuditableEntity, ITenantEntity
{
    public required string TruckNumber { get; set; }
    
    /// <summary>
    /// Truck last known location address
    /// </summary>
    public Address CurrentLocation { get; set; } = Address.NullAddress;
    
    /// <summary>
    /// Truck last known location latitude
    /// </summary>
    public double? CurrentLocationLat { get; set; }
    
    /// <summary>
    /// Truck last known location longitude
    /// </summary>
    public double? CurrentLocationLong { get; set; }

    public virtual List<Employee> Drivers { get; set; } = new();
    public virtual List<Load> Loads { get; } = new();

    public static Truck Create(string truckNumber, Employee driver)
    {
        return Create(truckNumber, new []{driver});
    }
    
    public static Truck Create(string truckNumber, IEnumerable<Employee> drivers)
    {
        var newTruck = new Truck
        {
            TruckNumber = truckNumber
        };

        newTruck.Drivers.AddRange(drivers);
        return newTruck;
    }

    /// <summary>
    /// The total percentage of income that drivers can receive from total gross income. Value must be in range [0, 1]
    /// </summary>
    public float GetDriversShareRatio()
    {
        return Drivers.Where(i => i.SalaryType == SalaryType.ShareOfGross).Sum(i => (float)i.Salary);
    }
}
