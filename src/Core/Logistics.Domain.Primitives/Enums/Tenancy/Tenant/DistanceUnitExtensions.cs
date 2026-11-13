namespace Logistics.Domain.Primitives.Enums;

/// <summary>Distances are stored in meters; these convert to the tenant's unit for pay and display.</summary>
public static class DistanceUnitExtensions
{
    private const double MetersPerMile = 1609.344;
    private const double MetersPerKilometer = 1000;

    extension(DistanceUnit unit)
    {
        public double FromMeters(double meters) =>
            unit == DistanceUnit.Kilometers ? meters / MetersPerKilometer : meters / MetersPerMile;

        public double ToMeters(double distance) =>
            unit == DistanceUnit.Kilometers ? distance * MetersPerKilometer : distance * MetersPerMile;

        public string Abbreviation() =>
            unit == DistanceUnit.Kilometers ? "km" : "mi";
    }

}
