using Logistics.Domain.Core;
using Logistics.Shared.Enums;

namespace Logistics.Domain.Entities;

public class Truck : AuditableEntity, ITenantEntity
{
    public string TruckNumber { get; set; } = default!;
    
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

    public static Truck Create(string truckNumber, Employee driver)
    {
        return Create(truckNumber, new []{driver});
    }
    
    public static Truck Create(string truckNumber, IEnumerable<Employee> drivers)
    {
        var newTruck = new Truck
        {
            TruckNumber = truckNumber,
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
