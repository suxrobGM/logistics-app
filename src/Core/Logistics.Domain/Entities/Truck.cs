using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

public class Truck : Entity, ITenantEntity
{
    public required string Number { get; set; }

    public required TruckType Type { get; set; }

    public TruckStatus Status { get; set; } = TruckStatus.Available;

    /// <summary>
    /// Truck's last known location address
    /// </summary>
    public required Address CurrentAddress { get; set; }

    /// <summary>
    /// Truck's last known location longitude
    /// </summary>
    public required GeoPoint CurrentLocation { get; set; }


    /// <summary>
    /// The main assigned driver for the truck
    /// </summary>
    public virtual Employee? MainDriver { get; set; }
    public Guid? MainDriverId { get; set; }

    /// <summary>
    /// The secondary (backup/co) driver for the truck
    /// </summary>
    public virtual Employee? SecondaryDriver { get; set; }
    public Guid? SecondaryDriverId { get; set; }

    //public virtual List<Employee> Drivers { get; set; } = [];
    public virtual List<Load> Loads { get; } = [];
    public virtual List<Trip> Trips { get; } = [];

    public static Truck Create(string truckNumber, TruckType type, Employee mainDriver, Employee? secondaryDriver = null)
    {
        return new Truck
        {
            Number = truckNumber,
            Type = type,
            MainDriver = mainDriver,
            SecondaryDriver = secondaryDriver,
            CurrentAddress = Address.NullAddress,
            CurrentLocation = GeoPoint.NullGeoPoint
        };
    }

    public IEnumerable<string> GetDriversNames()
    {
        if (MainDriver is { } mainDriver)
            yield return mainDriver.GetFullName();

        if (SecondaryDriver is { } secondaryDriver)
            yield return secondaryDriver.GetFullName();
    }

    /// <summary>
    /// The total percentage of income that drivers can receive from total gross income. Value must be in the range [0, 1]
    /// </summary>
    public float GetDriversShareRatio()
    {
        float ratio = 0;

        if (MainDriver?.SalaryType == SalaryType.ShareOfGross)
            ratio += (float)MainDriver.Salary.Amount;

        if (SecondaryDriver?.SalaryType == SalaryType.ShareOfGross)
            ratio += (float)SecondaryDriver.Salary.Amount;

        return ratio;
    }
}
