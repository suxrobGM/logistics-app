using Logistics.Domain.Core;
using Logistics.Domain.ValueObjects;
using Logistics.Shared.Consts;

namespace Logistics.Domain.Entities;

public class Truck : Entity, ITenantEntity
{
    public required string Number { get; set; }
    
    public required TruckType Type { get; set; }
    
    public TruckStatus Status { get; set; } = TruckStatus.Available;
    
    /// <summary>
    /// Truck's last known location address
    /// </summary>
    public Address CurrentLocation { get; set; } = Address.NullAddress;
    
    /// <summary>
    /// Truck's last known location longitude
    /// </summary>
    public double? CurrentLocationLong { get; set; }
    
    /// <summary>
    /// Truck's last known location latitude
    /// </summary>
    public double? CurrentLocationLat { get; set; }

    public virtual List<Employee> Drivers { get; set; } = [];
    public virtual List<Load> Loads { get; } = [];
    public virtual List<Trip> Trips { get; } = [];

    public static Truck Create(string truckNumber, TruckType type, Employee driver)
    {
        return Create(truckNumber, type, [driver]);
    }
    
    public static Truck Create(string truckNumber, TruckType type, IEnumerable<Employee> drivers)
    {
        var newTruck = new Truck
        {
            Number = truckNumber,
            Type = type,
        };

        newTruck.Drivers.AddRange(drivers);
        return newTruck;
    }

    /// <summary>
    /// The total percentage of income that drivers can receive from total gross income. Value must be in the range [0, 1]
    /// </summary>
    public float GetDriversShareRatio()
    {
        return (float)Drivers
            .Where(i => i.SalaryType == SalaryType.ShareOfGross)
            .Sum(i => i.Salary.Amount);
    }
}
