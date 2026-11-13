namespace Logistics.Domain.Primitives.Enums;

/// <summary>Distances are stored in meters and rates per km; these convert to the tenant's unit for pay and display.</summary>
public static class DistanceUnitExtensions
{
    private const double MetersPerKilometer = 1000;

    extension(DistanceUnit unit)
    {
        public double FromMeters(double meters) =>
            unit == DistanceUnit.Kilometers ? meters / MetersPerKilometer : meters / ImperialUnits.MetersPerMile;

        public double ToMeters(double distance) =>
            unit == DistanceUnit.Kilometers ? distance * MetersPerKilometer : distance * ImperialUnits.MetersPerMile;

        /// <summary>A stored rate per km, per this unit.</summary>
        public decimal RateFromPerKm(decimal ratePerKm) =>
            unit == DistanceUnit.Kilometers ? ratePerKm : ratePerKm * ImperialUnits.KilometersPerMile;

        /// <summary>A rate per this unit, per km for storage.</summary>
        public decimal RateToPerKm(decimal rate) =>
            unit == DistanceUnit.Kilometers ? rate : rate / ImperialUnits.KilometersPerMile;

        public string Abbreviation() =>
            unit == DistanceUnit.Kilometers ? "km" : "mi";
    }
}
