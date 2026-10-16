namespace Logistics.Domain.Primitives.Enums;

/// <summary>Distances are stored in meters; these convert to the tenant's unit for pay and display.</summary>
public static class DistanceUnitExtensions
{
    private const double MetersPerMile = 1609.344;
    private const double MetersPerKilometer = 1000;

    public static double FromMeters(this DistanceUnit unit, double meters) =>
        unit == DistanceUnit.Kilometers ? meters / MetersPerKilometer : meters / MetersPerMile;

    public static string Abbreviation(this DistanceUnit unit) =>
        unit == DistanceUnit.Kilometers ? "km" : "mi";
}
