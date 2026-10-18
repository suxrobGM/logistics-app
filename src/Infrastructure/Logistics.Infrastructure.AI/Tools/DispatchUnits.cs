using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Tools;

/// <summary>
/// Unit conversions shared by the dispatch tools. <c>GeoPoint.DistanceTo</c> and
/// <c>Load.Distance</c> are both in metres, but the model is shown kilometres or miles, so the
/// conversion happens in several tools and has to agree across them - the agent compares their
/// numbers against each other when it scores candidates.
/// </summary>
/// <remarks>
/// A distance key always ends in its unit (<c>_km</c>, <c>_mi</c>), which is the one rule the
/// prompts state: tools that report in the tenant's unit name it through <see cref="Suffix"/>.
/// </remarks>
internal static class DispatchUnits
{
    private const double MetersPerKm = 1000.0;
    private const double MilesPerKm = 0.621371;

    public static double MetersToKm(double meters) => meters / MetersPerKm;

    public static double KmToMiles(double km) => km * MilesPerKm;

    /// <summary>The key suffix for a distance in <paramref name="unit"/>.</summary>
    public static string Suffix(DistanceUnit unit) => unit == DistanceUnit.Kilometers ? "km" : "mi";

    public static double FromKm(double km, DistanceUnit unit) =>
        Math.Round(unit == DistanceUnit.Kilometers ? km : KmToMiles(km), 1);
}
