namespace Logistics.Domain.Primitives;

/// <summary>Exact US customary conversion factors, for data sources that publish miles, pounds and feet.</summary>
public static class ImperialUnits
{
    public const double MetersPerMile = 1609.344;
    public const decimal KilometersPerMile = 1.609344m;
    public const decimal KilogramsPerPound = 0.45359237m;
    public const decimal MetersPerFoot = 0.3048m;
}
