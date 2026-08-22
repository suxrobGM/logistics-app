using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.DbMigrator.Regions;

/// <summary>
/// <paramref name="StateCode"/> is the ISO 3166-2 subdivision code. Lane rate floor columns hold
/// three characters, so the display name in <paramref name="Address"/> cannot key a lane row.
/// </summary>
public record RoutePoint(Address Address, double Longitude, double Latitude, string StateCode);

public record TerminalSeed(string Name, string Code, string CountryCode, TerminalType Type, Address Address);

public record TruckMakeModel(string Make, string Model, string VinWmi);

public record VehicleMakeModel(string Make, string Model);

public record LicensePlate(string Number, string RegionCode);
