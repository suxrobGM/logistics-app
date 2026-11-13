using Logistics.Domain.Core;
using Logistics.Domain.Primitives;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

public class Truck : Entity, ITenantEntity
{
    private string _number = "";
    private string? _vin;

    /// <summary>
    ///     Trimmed on write; the casing is kept as typed, but numbers are unique regardless of case.
    /// </summary>
    public required string Number
    {
        get => _number;
        set => _number = value.Trim();
    }

    public required TruckType Type { get; set; }

    /// <summary>
    ///     Maximum number of vehicles that can be carried by the car hauler truck.
    /// </summary>
    public int VehicleCapacity { get; set; }

    public TruckStatus Status { get; set; } = TruckStatus.Available;

    /// <summary>
    ///     Manufacturer name (e.g., Freightliner, Peterbilt, Kenworth, Volvo)
    /// </summary>
    public string? Make { get; set; }

    /// <summary>
    ///     Model name (e.g., Cascadia, 579, T680, VNL)
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    ///     Year of manufacture
    /// </summary>
    public int? Year { get; set; }

    /// <summary>
    ///     Vehicle Identification Number (17-character unique identifier), canonicalised on write by
    ///     <see cref="VinFormat.Normalize" />.
    /// </summary>
    public string? Vin
    {
        get => _vin;
        set => _vin = VinFormat.Normalize(value);
    }

    /// <summary>
    ///     Vehicle license plate number
    /// </summary>
    public string? LicensePlate { get; set; }

    /// <summary>
    ///     State where the license plate is registered (e.g., "TX", "CA")
    /// </summary>
    public string? LicensePlateState { get; set; }

    /// <summary>
    ///     ADR (Accord Dangereux Routier) equipment configuration. Tracks whether this
    ///     truck is certified to carry hazardous goods and which classes it is approved for.
    /// </summary>
    public AdrEquipment AdrEquipment { get; set; } = AdrEquipment.None;

    /// <summary>
    ///     US-side flag: truck is placarded for Hazmat transport (per FMCSA / DOT).
    /// </summary>
    public bool IsHazmatPlacarded { get; set; }

    /// <summary>
    ///     Truck's last known location address
    /// </summary>
    public Address? CurrentAddress { get; set; }

    /// <summary>
    ///     Truck's last known location longitude
    /// </summary>
    public GeoPoint? CurrentLocation { get; set; }

    /// <summary>
    ///     The main assigned driver for the truck
    /// </summary>
    public virtual Employee? MainDriver { get; set; }

    public Guid? MainDriverId { get; set; }

    /// <summary>
    ///     The secondary (backup/co) driver for the truck
    /// </summary>
    public virtual Employee? SecondaryDriver { get; set; }

    public Guid? SecondaryDriverId { get; set; }

    /// <summary>
    ///     The trailer currently hooked to this truck. New trips default to it.
    /// </summary>
    public virtual Trailer? CurrentTrailer { get; set; }

    public Guid? CurrentTrailerId { get; set; }

    //public virtual List<Employee> Drivers { get; set; } = [];
    public virtual List<Load> Loads { get; } = [];
    public virtual List<Trip> Trips { get; } = [];
    public virtual List<TruckDocument> Documents { get; set; } = [];

    public static Truck Create(string truckNumber, TruckType type, Employee mainDriver,
        Employee? secondaryDriver = null)
    {
        return new Truck
        {
            Number = truckNumber,
            Type = type,
            MainDriver = mainDriver,
            SecondaryDriver = secondaryDriver
        };
    }

    /// <summary>
    ///     Hooks <paramref name="trailer" /> to this truck, or unhooks the current one when null. A trailer
    ///     can be hooked to one truck at a time, so the caller unhooks it from any other truck first.
    /// </summary>
    /// <exception cref="InvalidOperationException">The trailer is not Active.</exception>
    public void HookTrailer(Trailer? trailer)
    {
        trailer?.EnsureCanBeHooked();
        CurrentTrailer = trailer;
        CurrentTrailerId = trailer?.Id;
    }

    public bool IsDrivenBy(Guid driverId)
    {
        return MainDriverId == driverId || SecondaryDriverId == driverId;
    }

    /// <summary>Takes the driver out of whichever seat they hold on this truck.</summary>
    public void RemoveDriver(Guid driverId)
    {
        if (MainDriverId == driverId)
        {
            MainDriver = null;
            MainDriverId = null;
        }

        if (SecondaryDriverId == driverId)
        {
            SecondaryDriver = null;
            SecondaryDriverId = null;
        }
    }

    public IEnumerable<string> GetDriversNames()
    {
        if (MainDriver is { } mainDriver)
        {
            yield return mainDriver.GetFullName();
        }

        if (SecondaryDriver is { } secondaryDriver)
        {
            yield return secondaryDriver.GetFullName();
        }
    }

    /// <summary>
    ///     The total percentage of income that drivers can receive from total gross income. Value must be in the range [0, 1]
    /// </summary>
    public float GetDriversShareRatio()
    {
        float ratio = 0;

        if (MainDriver?.SalaryType == SalaryType.ShareOfGross)
        {
            ratio += (float)MainDriver.Salary.Amount;
        }

        if (SecondaryDriver?.SalaryType == SalaryType.ShareOfGross)
        {
            ratio += (float)SecondaryDriver.Salary.Amount;
        }

        return ratio;
    }
}
