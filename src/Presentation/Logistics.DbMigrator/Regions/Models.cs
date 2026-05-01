using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.DbMigrator.Regions;

public record RoutePoint(Address Address, double Longitude, double Latitude);

public record TerminalSeed(string Name, string Code, string CountryCode, TerminalType Type, Address Address);

public record TruckMakeModel(string Make, string Model, string VinWmi);

public record VehicleMakeModel(string Make, string Model);

public record LicensePlate(string Number, string RegionCode);
