namespace Logistics.Domain.Primitives.Enums;

/// <summary>Converts between stored units (distances in meters, rates per km) and the tenant's distance unit, for pay and display.</summary>
public static class DistanceUnitExtensions
{
    public const int MetersPerKilometer = 1000;

    extension(DistanceUnit unit)
    {
        public double FromMeters(double meters) =>
            unit == DistanceUnit.Kilometers ? meters / MetersPerKilometer : meters / ImperialUnits.MetersPerMile;

        public double ToMeters(double distance) =>
            unit == DistanceUnit.Kilometers ? distance * MetersPerKilometer : distance * ImperialUnits.MetersPerMile;

        /// <summary>Converts a stored rate per km into a rate per this unit.</summary>
        public decimal RateFromPerKm(decimal ratePerKm) =>
            unit == DistanceUnit.Kilometers ? ratePerKm : ratePerKm * ImperialUnits.KilometersPerMile;

        /// <summary>Converts a rate per this unit into a rate per km for storage.</summary>
        public decimal RateToPerKm(decimal rate) =>
            unit == DistanceUnit.Kilometers ? rate : rate / ImperialUnits.KilometersPerMile;

        public string Abbreviation() =>
            unit == DistanceUnit.Kilometers ? "km" : "mi";
    }
}
