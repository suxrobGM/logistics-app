using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.DbMigrator.Regions;

/// <summary>
/// Region-specific data and generators used by the seeders to produce realistic
/// fake data for either US or EU demo tenants.
/// </summary>
public interface IRegionProfile
{
    Region Region { get; }
    string DisplayName { get; }
    Address CompanyAddress { get; }
    CurrencyCode Currency { get; }

    /// <summary>Seeded onto a demo tenant, where it is the rate floor resolver's last fallback.</summary>
    decimal DefaultRateFloorPerMile { get; }

    DistanceUnit DistanceUnit { get; }
    WeightUnit WeightUnit { get; }
    VolumeUnit VolumeUnit { get; }
    TemperatureUnit TemperatureUnit { get; }
    DateFormatType DateFormat { get; }
    string Timezone { get; }

    IReadOnlyList<RoutePoint> RoutePoints { get; }
    IReadOnlyList<TerminalSeed> Terminals { get; }
    IReadOnlyList<TruckMakeModel> FreightTruckModels { get; }
    IReadOnlyList<TruckMakeModel> CarHaulerModels { get; }
    IReadOnlyList<TruckMakeModel> ContainerTruckModels { get; }

    IReadOnlyList<string> TripCorridorNames { get; }
    IReadOnlyList<string> ReeferCargoTypes { get; }
    IReadOnlyList<string> HazMatCargoTypes { get; }
    IReadOnlyList<string> TankCargoTypes { get; }
    IReadOnlyList<VehicleMakeModel> CarHaulerCargoVehicles { get; }
    IReadOnlyList<string> ContainerOwnerCodes { get; }
    IReadOnlyList<string> BrokerNames { get; }

    string GenerateVin(string make);
    LicensePlate GeneratePlate();
    string GenerateBrokerPhone();
}
