namespace Logistics.Infrastructure.AI.Tools;

/// <summary>
/// Unit conversions shared by the dispatch tools. <c>GeoPoint.DistanceTo</c> and
/// <c>Load.Distance</c> are both in metres, but the model is shown kilometres or miles, so the
/// conversion happens in several tools and has to agree across them - the agent compares their
/// numbers against each other when it scores candidates.
/// </summary>
internal static class DispatchUnits
{
    private const double MetersPerKm = 1000.0;
    private const double MilesPerKm = 0.621371;

    public static double MetersToKm(double meters) => meters / MetersPerKm;

    public static double KmToMiles(double km) => km * MilesPerKm;
}
