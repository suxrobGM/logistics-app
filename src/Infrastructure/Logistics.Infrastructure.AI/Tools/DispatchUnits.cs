using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Tools;

/// <summary>Unit conversions for agent tool output, shared so every tool reports the same numbers. Distance keys in tool output end in their unit.</summary>
internal static class DispatchUnits
{
    public static double MetersToKm(double meters) => meters / DistanceUnitExtensions.MetersPerKilometer;

    public static double FromMeters(double meters, DistanceUnit unit) => Math.Round(unit.FromMeters(meters), 1);

    /// <summary>Converts a stored rate per km to a rate per <paramref name="unit"/>, rounded to the cent; null stays null.</summary>
    public static decimal? RateFromPerKm(decimal? ratePerKm, DistanceUnit unit) =>
        ratePerKm is { } rate ? Math.Round(unit.RateFromPerKm(rate), 2) : null;

    /// <summary>Linehaul per distance unit, compared across tools when ranking candidates.</summary>
    public static double RatePer(decimal linehaul, double distance) =>
        distance > 0 ? Math.Round((double)linehaul / distance, 2) : 0;

    /// <summary>What both dispatch tools rank on; null when the pair has no estimate.</summary>
    public static double? MarginPer(decimal? margin, double distance) =>
        margin is { } m ? RatePer(m, distance) : null;

    /// <summary>The model ranks on <c>margin_per_{unit}</c>, so a pair with no estimate shows no margin keys.</summary>
    public static void AddMargin(Dictionary<string, object?> row, decimal? margin, double distance, DistanceUnit unit)
    {
        if (margin is null)
        {
            return;
        }

        row["est_margin"] = margin;
        row[$"margin_per_{unit.Abbreviation()}"] = MarginPer(margin, distance);
    }
}
