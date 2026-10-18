using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Tools;

/// <summary>
/// Metres (<c>GeoPoint.DistanceTo</c>, <c>Load.Distance</c>) to what the model is shown. Every tool
/// converts here so their numbers agree, and a distance key always ends in its unit
/// (<see cref="DistanceUnitExtensions.Abbreviation"/>) - the one unit rule the prompts state.
/// </summary>
internal static class DispatchUnits
{
    private const double MetersPerKm = 1000.0;

    public static double MetersToKm(double meters) => meters / MetersPerKm;

    public static double FromMeters(double meters, DistanceUnit unit) => Math.Round(unit.FromMeters(meters), 1);

    /// <summary>Linehaul per distance unit, compared across tools when ranking candidates.</summary>
    public static double RatePer(decimal linehaul, double distance) =>
        distance > 0 ? Math.Round((double)linehaul / distance, 2) : 0;
}
