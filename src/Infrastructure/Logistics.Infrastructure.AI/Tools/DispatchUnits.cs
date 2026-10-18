using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Tools;

/// <summary>
/// Metres (<c>GeoPoint.DistanceTo</c>, <c>Load.Distance</c>) to what the model is shown. Every tool
/// converts here so their numbers agree, and a distance key always ends in its unit
/// (<see cref="Suffix"/>) - the one unit rule the prompts state.
/// </summary>
internal static class DispatchUnits
{
    private const double MetersPerKm = 1000.0;
    private const double MilesPerKm = 0.621371;

    public static double MetersToKm(double meters) => meters / MetersPerKm;

    public static double KmToMiles(double km) => km * MilesPerKm;

    public static string Suffix(DistanceUnit unit) => unit == DistanceUnit.Kilometers ? "km" : "mi";

    public static double FromKm(double km, DistanceUnit unit) =>
        Math.Round(unit == DistanceUnit.Kilometers ? km : KmToMiles(km), 1);
}
